using FaceitParser.Abstractions;
using FaceitParser.Data;
using FaceitParser.Models;
using System.Collections.Concurrent;
using System.Text;
using System.Threading;
using System.Xml.Linq;

namespace FaceitParser.Services
{
    public class FaceitService : IFaceitService, IDisposable
    {
        const int MAX_FACEIT_PLAYERS = 100;

        const int MIN_FACEIT_PLAYERS = 30;

        const int LOOP_DELAY = 1000;


        public string Name { get; set; }

        public int Delay { get; set; }


        public int Games;

        public int Total;

        public int Parsed;

        public int Added;


        public ConcurrentQueue<string> Logs { get; set; }

        public FaceitApi faceitApi { get; set; }


        private Dictionary<string, double> _items { get; set; }

        private ISteamApi _steamApi { get; set; }

        private int _maxLevel { get; set; }

        private Location _location { get; set; }

        private CancellationToken _cancellationToken { get; set; }

        private ConcurrentQueue<Player> _players { get; set; }

        private double _minPrice { get; set; }

        private ApplicationDbContext _context { get; set; }

        private string _userId { get; set; }

        public FaceitService(ISteamApi steamApi, string name,  Location location, FaceitApi faceitapi, int delay, int maxLvl, int minPrice, ApplicationDbContext dbContext, string userId, CancellationToken cancellationToken)
        {
            _userId = userId;
            _steamApi = steamApi;
            _location = location;
            _maxLevel = maxLvl;
            _cancellationToken = cancellationToken;
            faceitApi = faceitapi;
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
            /*_items = await new SteamApi("qE4I4rd6hcjPl8CwYp4fW0Z4Lzc").GetItems();*/
            Log($"Авторизованы как {faceitApi.SelfNick}");
            Log($"Получено {_items.Count()} предметов с маркета");
        }

        public async Task Start()
        {
            Games = 0;
            Total = 0;
            Parsed = 0;
            Task.Run(async () => await LoopGames());
            Task.Run(async () => await LoopPlayers());
        }

        public async Task LoopGames()
        {
            int offset = 0;
            while (!_cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var gameIds = await faceitApi.GetGameIdsAsync(_location.Region, offset);
                    if (!gameIds.Any()) {
                        offset = 0;
                        Log("Начинаем парсинг с начала");
                        continue;
                    }
                    await Task.Delay(Delay);
                    foreach (var gameId in gameIds)
                    {
                        Interlocked.Increment(ref Games);

                        var initPlayers = await faceitApi.GetPlayersAsync(gameId, _maxLevel);
                        if (!initPlayers.Any())
                            continue;
                        Interlocked.Add(ref Total, initPlayers.Count());
                        await Task.Delay(Delay);

                        var players = await faceitApi.GetPlayersAsync(initPlayers, _location.Countries, _location.IgnoreCountries);
                        if (!players.Any())
                            continue;
                        var userBlacklist = _context.Blacklists.FirstOrDefault(x => x.UserId == _userId);
                        foreach (var player in players)
                        {   
                            if (userBlacklist?.Players?.Any(x => x.ProfileId == player.ProfileId) ?? false)
                                continue;
                            var price = await GetInventoryPrice(player);
                            if (price >= _minPrice)
                            {
                                Log($"Спарсили {player.Nick} - {player.Level} LVL, {player.Country}, {price}$");
                                //_players.Enqueue(player);
                            }
                        }
                        if (userBlacklist is null)
                            await _context.Blacklists.AddAsync(new BlacklistModel(_userId, players.ToList()));
                        else
                            userBlacklist.Players.AddRange(players);
                        await _context.SaveChangesAsync(_cancellationToken);
                        Interlocked.Add(ref Parsed, players.Count());
                    }
                    offset += gameIds.Count();
                } 
                catch (Exception ex)
                {
                    Log(ex.Message);
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
                    List<Player> chunkPlayers = new List<Player>();
                    int count = 0;
                    while (count < MAX_FACEIT_PLAYERS && _players.TryDequeue(out Player result))
                    {
                        chunkPlayers.Add(result);
                        count++;
                    }
                    await faceitApi.AddFriendsAsync(chunkPlayers);
                    chunkPlayers.ForEach(player =>
                    {
                        Log($"Добавили {player.Nick}");
                    });
                }
                await Task.Delay(LOOP_DELAY);
            }
        }

        private void Log(string message)
        {
            var date = DateTime.Now.ToString("hh:mm:ss");
            Logs.Enqueue($"[{date}] {message}");
        }

        public void Dispose()
        {
            _context.Dispose();
            faceitApi.Dispose();
        }
    }
}
