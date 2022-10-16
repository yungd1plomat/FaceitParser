using FaceitParser.Abstractions;
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



        private Dictionary<string, double> _items { get; set; }

        private ISteamApi _steamApi { get; set; }

        private int _maxLevel { get; set; }

        private Location _location { get; set; }

        private FaceitApi _faceitApi { get; set; }

        private CancellationToken _cancellationToken { get; set; }

        private ConcurrentQueue<Player> _players { get; set; }


        public FaceitService(ISteamApi steamApi, string name,  Location location, FaceitApi faceitApi, int delay, int maxLvl, CancellationToken cancellationToken)
        {
            _steamApi = steamApi;
            _location = location;
            _maxLevel = maxLvl;
            _faceitApi = faceitApi;
            _cancellationToken = cancellationToken;
            Name = name;
            Delay = delay;
        }

        public async Task Init()
        {
            //_items = await _steamApi.GetItems(); Не забыть поменять
            _items = await new SteamApi("qE4I4rd6hcjPl8CwYp4fW0Z4Lzc").GetItems();
        }

        public async Task Start()
        {
            Games = 0;
            Total = 0;
            Parsed = 0;
            Logs = new ConcurrentQueue<string>();
            _players = new ConcurrentQueue<Player>();
            await LoopGames().ConfigureAwait(false);
            await LoopPlayers().ConfigureAwait(false);
        }

        public async Task LoopGames()
        {
            int offset = 0;
            while (!_cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var gameIds = await _faceitApi.GetGameIdsAsync(_location.Region, offset);
                    if (!gameIds.Any()) {
                        offset = 0;
                        Logs.Enqueue("Начинаем парсинг с начала");
                        continue;
                    }
                    await Task.Delay(Delay);
                    foreach (var gameId in gameIds)
                    {
                        Interlocked.Increment(ref Games);

                        var initPlayers = await _faceitApi.GetPlayersAsync(gameId, _maxLevel);
                        if (!initPlayers.Any())
                            continue;
                        Interlocked.Add(ref Total, initPlayers.Count());
                        await Task.Delay(Delay);

                        var players = await _faceitApi.GetPlayersAsync(initPlayers, _location.Countries, _location.IgnoreCountries);
                        if (!players.Any())
                            continue;
                        Interlocked.Add(ref Parsed, players.Count());

                    }
                    offset += gameIds.Count();
                    await Task.Delay(Delay);
                } 
                catch (Exception ex)
                {
                    Logs.Enqueue(ex.Message);
                }
            }
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
                    await _faceitApi.AddFriendsAsync(chunkPlayers);
                }
                await Task.Delay(LOOP_DELAY);
            }
        }

        public void Dispose()
        {
            _faceitApi.Dispose();
        }
    }
}
