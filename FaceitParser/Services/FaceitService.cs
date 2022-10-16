using FaceitParser.Models;
using System.Collections.Concurrent;
using System.Text;

namespace FaceitParser.Services
{
    public class FaceitService : IDisposable
    {
        public string Name { get; set; }

        public int Delay { get; set; }

        public int Games { get; set; }

        public int Total { get; set; }

        public int Parsed { get; set; }

        public ConcurrentQueue<string> Logs { get; set; }



        private Dictionary<string, double> _items { get; set; }

        private SteamApi _steamApi { get; set; }

        private int _maxLevel { get; set; }

        private Location _location { get; set; }

        private FaceitApi _faceitApi { get; set; }


        public FaceitService(SteamApi steamApi, Location location, FaceitApi faceitApi, int delay, int maxLvl)
        {
            _steamApi = steamApi;
            _location = location;
            _maxLevel = maxLvl;
            _faceitApi = faceitApi;
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
            int offset = 0;
            while (true)
            {
                var gameIds = await _faceitApi.GetGameIdsAsync(_location.Region, offset);
                await Task.Delay(Delay);
                foreach (var gameId in gameIds)
                {
                    var initPlayers = await _faceitApi.GetPlayersAsync(gameId, _maxLevel);
                    Total += initPlayers.Count();
                    await Task.Delay(Delay);
                    if (!initPlayers.Any())
                        continue;
                    var players = await _faceitApi.GetPlayersAsync(initPlayers, _location.Countries, _location.IgnoreCountries);
                    foreach (var player in players)
                    {
                        Console.WriteLine(player.Nick + ":" + player.Country + ":" + player.Level);
                        await File.AppendAllLinesAsync("players.txt", players.Select(x => x.Id), Encoding.UTF8);
                        Parsed++;
                    }
                    Games++;
                    Console.Title = $"{Games} | {Total} | {Parsed}";
                }
                Console.Title = $"{Games} | {Total} | {Parsed}";
                offset += gameIds.Count();
                await Task.Delay(Delay);

            }
        }

        public void Dispose()
        {
            _faceitApi.Dispose();
        }
    }
}
