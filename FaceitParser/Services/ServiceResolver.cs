using FaceitParser.Abstractions;
using FaceitParser.Data;
using FaceitParser.Helpers;
using FaceitParser.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Threading;

namespace FaceitParser.Services
{
    public class ServiceResolver : IServiceResolver
    {
        private ISteamApi _steamApi { get; set; }

        private IConfiguration _configuration { get; set; }

        public IDictionary<string, IDictionary<IFaceitService, CancellationTokenSource>> Services { get; set; }

        

        public ServiceResolver(ISteamApi steamApi, IConfiguration configuration)
        {
            _configuration = configuration;
            _steamApi = steamApi;
            Services = new Dictionary<string, IDictionary<IFaceitService, CancellationTokenSource>>();
        }


        public IEnumerable<IFaceitService> Resolve(string user, string name = null)
        {
            if (!Services.ContainsKey(user))
                return Array.Empty<FaceitService>();
            if (name is null)
                return Services[user].Keys;
            return Services[user].Keys.Where(x => x.Name == name);
        }

        public async Task Create(string user, string name, FaceitApi faceitApi, Location location, int delay, int maxLvl, int maxMatches, int minPrice, bool autoAdd, CancellationTokenSource source)
        {
            // Для использования в разных потоках (чтобы не было конфликтов)
            string connectionString = _configuration["CONNECTION_STRING"];
            var playersDb = ContextFactory.CreateContext(connectionString);
            var friendsDb = ContextFactory.CreateContext(connectionString);
            FaceitService faceitService = new FaceitService(_steamApi, name, location, faceitApi, delay, maxLvl, maxMatches, minPrice, autoAdd, playersDb, friendsDb, user, source.Token);
            await faceitService.Init();
            await faceitService.Start().ConfigureAwait(false);
            if (!Services.ContainsKey(user))
            {
                Services.Add(user, new Dictionary<IFaceitService, CancellationTokenSource>());
            }
            Services[user].Add(faceitService, source);
        }

        public void Remove(string user, string name)
        {
            if (!Services.ContainsKey(user))
            {
                return;
            }
            var service = Services[user].FirstOrDefault(x => x.Key.Name == name);
            if (service.Key == null || service.Value == null)
                return;
            var cancelToken = service.Value;
            var parser = service.Key as FaceitService;
            cancelToken.Cancel();
            parser.Dispose();
            Services[user].Remove(service);
        }


    }
}
