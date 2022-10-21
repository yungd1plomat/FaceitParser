using FaceitParser.Abstractions;
using FaceitParser.Data;
using FaceitParser.Models;
using System;
using System.Collections.Concurrent;
using System.Text;
using System.Threading;
using System.Xml.Linq;

namespace FaceitParser.Services
{
    public class FaceitService : IFaceitService, IDisposable
    {
        const int MAX_FACEIT_PLAYERS = 100;

        const int MIN_FACEIT_PLAYERS = 15;

        const int LOOP_DELAY = 1000;


        public string Name { get; set; }

        public int Delay { get; set; }


        public int Games;

        public int Total;

        public int Parsed;

        public int Added;


        public ConcurrentQueue<string> Logs { get; set; }

        public FaceitApi FaceitApi { get; set; }

        public Location Location { get; set; }


        private Dictionary<string, double> _items { get; set; }

        private ISteamApi _steamApi { get; set; }

        private int _maxLevel { get; set; }


        private CancellationToken _cancellationToken { get; set; }

        private ConcurrentQueue<Player> _players { get; set; }

        private double _minPrice { get; set; }

        private ApplicationDbContext _context { get; set; }

        private string _userId { get; set; }

        private Semaphore semaphore { get; set; }

        public FaceitService(ISteamApi steamApi, string name,  Location location, FaceitApi faceitapi, int delay, int maxLvl, int minPrice, ApplicationDbContext dbContext, string userId, CancellationToken cancellationToken)
        {
            semaphore = new Semaphore(1, 1);
            _userId = userId;
            _steamApi = steamApi;
            Location = location;
            _maxLevel = maxLvl;
            _cancellationToken = cancellationToken;
            FaceitApi = faceitapi;
            Name = name;
            Delay = delay;
            Logs = new ConcurrentQueue<string>();
            _players = new ConcurrentQueue<Player>();
            _minPrice = minPrice;
            _context = dbContext;
        }

        public async Task Init()
        {
            _items = await _steamApi.GetItems();
            Log($"Авторизованы как {FaceitApi.SelfNick}");
            Log($"Получено {_items.Count()} предметов с маркета");
        }

        public async Task Start()
        {
            Games = 0;
            Total = 0;
            Parsed = 0;
            new Thread(async () => await LoopGames()).Start();
            new Thread(async () => await LoopPlayers()).Start();
        }

        public async Task LoopGames()
        {
            int offset = 0;
            while (!_cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var gameIds = await FaceitApi.GetGameIdsAsync(Location.Region, offset);
                    if (gameIds is null || !gameIds.Any()) {
                        offset = 0;
                        Log("Игр не найдено, начинаем парсинг с начала");
                        await Task.Delay(Delay);
                        continue;
                    }
                    await Task.Delay(Delay);
                    foreach (var gameId in gameIds)
                    {
                        if (_cancellationToken.IsCancellationRequested)
                            break;
                        Interlocked.Increment(ref Games);

                        var initPlayers = await FaceitApi.GetPlayersAsync(gameId, _maxLevel);
                        if (initPlayers is null || !initPlayers.Any())
                        {
                            await Task.Delay(Delay);
                            continue;
                        }
                        Interlocked.Add(ref Total, initPlayers.Count());
                        await Task.Delay(Delay);

                        var players = await FaceitApi.GetPlayersAsync(initPlayers, Location.Countries, Location.IgnoreCountries);
                        if (players is null || !players.Any())
                        {
                            await Task.Delay(Delay);
                            continue;
                        }
                        var userBlacklist = _context.Blacklists.Where(x => x.UserId == _userId).ToList();
                        List<Thread> threads = new List<Thread>();
                        foreach (var player in players)
                        {
                            if (_cancellationToken.IsCancellationRequested)
                                break;
                            var thread = new Thread(async () =>
                            {
                                if (userBlacklist?.Any(x => x.ProfileId == player.ProfileId) == true)
                                    return;
                                var price = await GetInventoryPrice(player).ConfigureAwait(false);
                                if (price >= _minPrice)
                                {
                                    Log($"Спарсили {player.Nick} - {player.Level} LVL, {player.Country}, {price}$");
                                    _players.Enqueue(player);
                                    Interlocked.Increment(ref Parsed);
                                    var model = new BlacklistModel()
                                    {
                                        ProfileId = player.ProfileId,
                                        UserId = _userId,
                                    };
                                    if (!_cancellationToken.IsCancellationRequested)
                                    {
                                        semaphore.WaitOne();
                                        await _context.Blacklists.AddAsync(model);
                                        semaphore.Release();
                                    }
                                }
                            });
                            threads.Add(thread);
                            thread.Start();
                        }
                        foreach (var thread in threads)
                            thread.Join();
                        await _context.SaveChangesAsync(_cancellationToken);
                    }
                    offset += gameIds.Count();
                } 
                catch (Exception ex)
                {
                    Log($"{ex.Message}:{ex.StackTrace}");
                }
                await Task.Delay(Delay);
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
            return price;
        }

        public async Task LoopPlayers()
        {
            while (!_cancellationToken.IsCancellationRequested)
            {
                if (_players.Count > MIN_FACEIT_PLAYERS)
                {
                    try
                    {
                        List<Player> chunkPlayers = new List<Player>();
                        int count = 0;
                        while (count < MAX_FACEIT_PLAYERS && _players.TryDequeue(out Player result))
                        {
                            chunkPlayers.Add(result);
                            count++;
                        }
                        await FaceitApi.AddFriendsAsync(chunkPlayers);
                        chunkPlayers.ForEach(player =>
                        {
                            Log($"Добавили {player.Nick}");
                        });
                        Interlocked.Add(ref Added, chunkPlayers.Count());
                    } catch (Exception ex)
                    {
                        Log(ex.Message);
                    }
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
            Logs.Clear();
            _players.Clear();
            _items.Clear();
            _context.Dispose();
            FaceitApi.Dispose();
        }
    }
}
