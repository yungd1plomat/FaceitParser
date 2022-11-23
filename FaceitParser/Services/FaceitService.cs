using FaceitParser.Abstractions;
using FaceitParser.Data;
using FaceitParser.Helpers.Extensions;
using FaceitParser.Models;
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

        const int QUEUE_LIMIT = 20;

        const int DELAYS_COUNT = 3;

        const int INACCURACY = 3000;


        public string Name { get; private set; }

        public int Delay { get; private set; }

        public ConcurrentInt Games { get; private set; }

        public ConcurrentInt Total { get; private set; }

        public ConcurrentInt Parsed { get; private set; }

        public ConcurrentInt Added { get; private set; }


        public ConcurrentQueue<string> Logs { get; set; }

        public ConcurrentQueue<ulong> SteamIds { get; set; }

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

        private ConcurrentQueue<Player> _players { get; set; }

        private ApplicationDbContext _playersContext { get; set; }

        private ApplicationDbContext _friendsContext { get; set; }


        private FaceitAccount _account { get; set; }


        private bool _limited { get; set; }

        private bool _needRestart { get; set; }

        private int _defaultDelay { get; set; }


        public FaceitService(ISteamApi steamApi, string name,  Location location, IFaceitApi faceitapi, int delay, int maxLvl, int maxMatches, int minPrice, bool autoAdd, ApplicationDbContext playersContext, ApplicationDbContext friendsContext, string userId, CancellationToken cancellationToken)
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
            SteamIds = new ConcurrentQueue<ulong>();
            _players = new ConcurrentQueue<Player>();
            _minPrice = minPrice;
            _friendsContext = friendsContext;
            _playersContext = playersContext;
            _limited = false;
            _needRestart = false;
            _defaultDelay = delay;
            Added = new ConcurrentInt();
            Games = new ConcurrentInt();
            Total = new ConcurrentInt();
            Parsed = new ConcurrentInt();
        }

        public async Task Init()
        {
            _account = await _friendsContext.Accounts.FirstAsync(x => x.Token == _faceitApi.Token);
            _items = await _steamApi.GetItems();
            Log($"Авторизованы как {_faceitApi.SelfNick}");
            Log($"Получено {_items.Count()} предметов с маркета");
        }

        public async Task Start()
        {
            new Thread(async () => await LoopGames()).Start();
            if (_autoAdd)
                new Thread(async () => await LoopPlayers()).Start();
        }

        public async Task LoopGames()
        {
            int offset = 0;
            while (!_cancellationToken.IsCancellationRequested && !_limited && !_needRestart)
            {
                try
                {
                    var gameIds = await _faceitApi.GetGameIdsAsync(Location.Region, offset);
                    if (gameIds is null || !gameIds.Any()) {
                        offset = 0;
                        Log("Игр не найдено, начинаем парсинг с начала");
                        await Task.Delay(Delay);
                        continue;
                    }
                    await Task.Delay(Delay);
                    foreach (var gameId in gameIds)
                    {
                        if (_cancellationToken.IsCancellationRequested || _limited)
                            break;
                        await ProcessGame(gameId);
                        Games.Increment();
                        await Task.Delay(Delay);
                    }
                    offset += gameIds.Count();
                } 
                catch (Exception ex)
                {
                    Log($"{ex.Message}:{ex.StackTrace}");
                    ProcessExceptions(ex);
                }
                await Task.Delay(Delay);
            }
        }

        private void ProcessExceptions(Exception ex)
        {
            if (ex.Message.ToLower().Contains("cannot open when state is connecting"))
            {
                _needRestart = true;
                Log($"Требуется перезапуск парсера..");
            }
        }

        private async Task ProcessGame(string gameId)
        {
            var initPlayers = await _faceitApi.GetPlayersAsync(gameId, _maxLevel);
            if (initPlayers is null || !initPlayers.Any()) 
                return;
            await Task.Delay(Delay);

            var players = await _faceitApi.GetPlayersAsync(initPlayers, Location.Countries, Location.IgnoreCountries);
            if (players is null || !players.Any())
                return;

            var userBlacklist = _playersContext.Blacklists.Where(x => x.UserId == _userId).ToList();
            List<Thread> threads = new List<Thread>();
            foreach (var player in players)
            {
                if (_cancellationToken.IsCancellationRequested || _limited)
                    break;
                var thread = new Thread(async () =>
                {
                    if (userBlacklist?.Any(x => x.ProfileId == player.ProfileId) == true)
                        return;
                    var price = await GetInventoryPrice(player).ConfigureAwait(false);
                    if (price >= _minPrice)
                    {
                        Log($"Спарсили {player.Nick} - {player.Level} LVL, {player.Country}, {price}$");
                        SteamIds.Enqueue(player.ProfileId);
                        if (_autoAdd)
                            _players.Enqueue(player);
                        Parsed.Increment();
                    }
                });
                threads.Add(thread);
                thread.Start();
            }
            foreach (var thread in threads)
                thread.Join();
            Total.Add(initPlayers.Count());
        }

        private void CalculateDelay()
        {
            if (_players.Count == 0)
                Delay = _defaultDelay;
            if (_players.Count >= QUEUE_LIMIT)
            {
                // Получаем делей, при котором парсер дождется добавления в друзья всех игроков
                int stopDelay = ((_players.Count * LOOP_DELAY) / DELAYS_COUNT) + INACCURACY;
                Delay = stopDelay;
            }
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

        public async Task LoopPlayers()
        {
            while (!_cancellationToken.IsCancellationRequested && !_limited && !_needRestart)
            {
                if (!_players.TryDequeue(out Player player))
                    continue;
                CalculateDelay();
                try
                {
                    if (_account.FriendRequests >= LIMIT)
                    {
                        _limited = true;
                        Log($"Достигнут лимит друзей на аккаунте {_account.FriendRequests}/{LIMIT}");
                        break;
                    }
                    await _faceitApi.AddFriendsAsync(new Player[] { player });
                    Log($"Добавили {player.Nick}");
                    var model = new BlacklistModel()
                    {
                        ProfileId = player.ProfileId,
                        UserId = _userId,
                    };
                    await _friendsContext.Blacklists.AddAsync(model);
                    _account.FriendRequests++;
                    await _friendsContext.SaveChangesAsync(_cancellationToken);
                    Added.Increment();
                }
                catch (Exception ex)
                {
                    _players.Enqueue(player);
                    Log(ex.Message);
                }
                await Task.Delay(LOOP_DELAY);
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
            _players.Clear();
            _items.Clear();
            _friendsContext.Dispose();
            _playersContext.Dispose();
        }
    }
}
