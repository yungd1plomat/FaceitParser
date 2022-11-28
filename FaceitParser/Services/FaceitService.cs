using FaceitParser.Abstractions;
using FaceitParser.Data;
using FaceitParser.Helpers;
using FaceitParser.Helpers.Extensions;
using FaceitParser.Models;
using FaceitParser.Models.App;
using FaceitParser.Models.Faceit;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Xml.Linq;

namespace FaceitParser.Services
{
    public class FaceitService : IFaceitService, IDisposable
    {
        public const int LIMIT = 1000;

        const int LOOP_DELAY = 4000;

        const int PARSE_LIMIT = 1000;

        const int BLACKLIST_DELAY = 5000;

        public string Name { get; private set; }

        public int Delay { get; private set; }

        public ConcurrentInt Games { get; private set; }

        public ConcurrentInt Total { get; private set; }

        public ConcurrentInt Parsed { get; private set; }

        public ConcurrentInt Added { get; private set; }


        public ConcurrentQueue<string> Logs { get; set; }

        public ConcurrentQueue<string> SteamIds { get; set; }

        public Location Location { get; set; }

        public string AccountNick { get => _faceitApi.SelfNick; }


        private Dictionary<string, double> _items { get; set; }

        private IFaceitApi _faceitApi { get; set; }

        private ISteamApi _steamApi { get; set; }


        private int _maxLevel { get; set; }

        private double _minPrice { get; set; }

        private int _maxMatches { get; set; }

        private bool _autoAdd { get; set; }

        private string _userId { get; set; }


        private CancellationToken _cancellationToken { get; set; }

        private ApplicationDbContext _context { get; set; }


        private FaceitAccount _account { get; set; }

        private ConcurrentQueue<Player> _playersQueue { get; set; }

        private ConcurrentQueue<BlacklistModel> _blacklistsQueue { get; set; }

        private object _locker = new object();

        private bool _friendsLimit { get; set; }

        private bool _parseLimit { get; set; }

        private bool _needRestart { get; set; }


        public FaceitService(ISteamApi steamApi, string name,  Location location, IFaceitApi faceitapi, int delay, int maxLvl, int maxMatches, int minPrice, bool autoAdd, ApplicationDbContext context, string userId, CancellationToken cancellationToken)
        {
            _userId = userId;
            _steamApi = steamApi;
            Location = location;
            _maxLevel = maxLvl;
            _maxMatches = maxMatches;
            _autoAdd = autoAdd;
            _cancellationToken = cancellationToken;
            _faceitApi = faceitapi;
            Name = name;
            Delay = delay;
            Logs = new ConcurrentQueue<string>();
            SteamIds = new ConcurrentQueue<string>();
            _playersQueue = new ConcurrentQueue<Player>();
            _blacklistsQueue = new ConcurrentQueue<BlacklistModel>();
            _minPrice = minPrice;
            _context = context;
            _friendsLimit = false;
            _parseLimit = false;
            _needRestart = false;
            Added = new ConcurrentInt();
            Games = new ConcurrentInt();
            Total = new ConcurrentInt();
            Parsed = new ConcurrentInt();
            
        }

        public async Task Init()
        {
            _account = await _context.Accounts.FirstAsync(x => x.Token == _faceitApi.Token);
            _items = await _steamApi.GetItems();
            Log($"Авторизованы как {_faceitApi.SelfNick}");
            Log($"Получено {_items.Count()} предметов с маркета");
        }

        public Task Start()
        {
            Task.Factory.StartNew(LoopGames, TaskCreationOptions.LongRunning);
            Task.Factory.StartNew(LoopBlacklist, TaskCreationOptions.LongRunning);
            if (_autoAdd)
            {
                Task.Factory.StartNew(LoopPlayers, TaskCreationOptions.LongRunning);
            }
            return Task.CompletedTask;
        }

        public async Task LoopGames()
        {
            int offset = 0;
            while (!_cancellationToken.IsCancellationRequested && !_parseLimit && !_friendsLimit && !_needRestart)
            {
                try
                {
                    var gameIds = await _faceitApi.GetGameIdsAsync(Location.RegionId, offset);
                    if (gameIds is null || !gameIds.Any()) {
                        offset = 0;
                        Log("Игр не найдено, начинаем парсинг с начала");
                        await Task.Delay(Delay);
                        continue;
                    }
                    await Task.Delay(Delay);
                    foreach (var gameId in gameIds)
                    {
                        if (Parsed.value >= PARSE_LIMIT)
                        {
                            _parseLimit = true;
                            Log($"Достигнут лимит парсинга для текущей инстанции {Parsed.value}/{PARSE_LIMIT}, перезапустите парсер");
                        }
                        if (_cancellationToken.IsCancellationRequested || _parseLimit || _friendsLimit)
                            break;
                        await ProcessGame(gameId);
                        Games.Increment();
                        var delay = _playersQueue.IsEmpty ? Delay : LOOP_DELAY * _playersQueue.Count + Delay;
                        await Task.Delay(delay);
                    }
                    offset += gameIds.Count();
                } 
                catch (Exception ex)
                {
                    Log($"{ex.Message}:{ex.StackTrace}");
                    ProcessExceptions(ex);
                }
            }
        }

        public async Task LoopBlacklist()
        {
            while (!_cancellationToken.IsCancellationRequested && !_needRestart)
            {
                if (!_blacklistsQueue.IsEmpty)
                {
                    int count = 0;
                    lock (_locker)
                    {
                        while (_blacklistsQueue.TryDequeue(out var blacklistModel))
                        {
                            _context.Blacklists.Add(blacklistModel);
                            count++;
                        }
                        _context.SaveChanges();
                    }
                }
                await Task.Delay(BLACKLIST_DELAY);
            }
        }

        public async Task LoopPlayers()
        {
            while (!_cancellationToken.IsCancellationRequested &&
                   _autoAdd &&
                   !_friendsLimit &&
                   !_needRestart)
            {
                if (!_playersQueue.TryDequeue(out var player))
                    continue;
                try
                {
                    await _faceitApi.AddFriendsAsync(new Player[] { player });
                    Added.Increment();
                    Log($"Добавили {player.Nick}");
                    lock (_locker)
                    {
                        _account.FriendRequests++;
                        _context.SaveChanges();
                    }
                }
                catch (Exception ex)
                {
                    _playersQueue.Enqueue(player);
                    Log(ex.Message);
                }
                if (_account.FriendRequests >= LIMIT)
                {
                    _friendsLimit = true;
                    Log($"Достигнут лимит друзей на аккаунте {_account.FriendRequests}/{LIMIT}");
                    break;
                }
                await Task.Delay(LOOP_DELAY);
            }
        }

        public async Task ProcessGame(string gameId)
        {
            var initPlayers = await _faceitApi.GetPlayersAsync(gameId, _maxLevel);
            if (initPlayers is null || !initPlayers.Any())
                return;
            await Task.Delay(Delay);

            var players = await _faceitApi.GetPlayersAsync(initPlayers, Location.Countries, Location.IgnoreCountries);
            if (players is null || !players.Any())
                return;

            if (_maxMatches != 0)
            {
                await Task.Delay(Delay);
                players = await _faceitApi.GetPlayersAsync(players, gameId, _maxMatches);
            }
            List<BlacklistModel> userBlacklist = null;
            lock (_locker)
            {
                userBlacklist = _context.Blacklists.Where(x => x.UserId == _userId).ToList();
            }
            foreach (var player in players)
            {
                if (_cancellationToken.IsCancellationRequested || _friendsLimit || _parseLimit)
                    break;
                if (userBlacklist.Any(x => x.ProfileId == player.ProfileId))
                    continue;
                new Thread(async () =>
                {
                    var price = await GetInventoryPrice(player);
                    if (price >= _minPrice)
                    {
                        var matches = player.Matches is null ? "not specified" : player.Matches.ToString();
                        Log($"Спарсили {player.Nick} - {player.Level} LVL, {player.Country}, {matches} matches, {price}$");
                        SteamIds.Enqueue(player.ProfileId.ToString());
                        _blacklistsQueue.Enqueue(new BlacklistModel()
                        {
                            UserId = _userId,
                            ProfileId = player.ProfileId,
                        });
                        if (_autoAdd)
                        {
                            _playersQueue.Enqueue(player);
                        }
                        Parsed.Increment();
                    }
                }).Start();
            }
            Total.Add(initPlayers.Count());
        }

        public async Task<double> GetInventoryPrice(Player player)
        {
            if (_minPrice == 0)
                return 0;
            double price = 0;
            try
            {
                var inventory = await _steamApi.GetInventory(player.ProfileId);
                var items = inventory.Items.Where(x => x.Tradable);
                foreach (var item in items)
                {
                    if (_items.ContainsKey(item.Name))
                    {
                        price += _items[item.Name];
                    }
                }
            } catch { }
            return Math.Round(price, 3);
        }

        private void ProcessExceptions(Exception ex)
        {
            if (ex.Message.ToLower().Contains("cannot open when state is connecting"))
            {
                _needRestart = true;
                Log($"Требуется перезапуск парсера..");
            }
        }

        private void Log(string message)
        {
            var date = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time")).ToString("hh:mm:ss");
            Logs.Enqueue($"[{date}] {message}");
        }

        public void Dispose()
        {
            var faceitApi = (_faceitApi as FaceitApi);
            faceitApi.Dispose();
            Logs.Clear();
            _items.Clear();
            _context.Dispose();
        }


    }
}
