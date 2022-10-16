using FaceitParser.Abstractions;
using FaceitParser.Models;
using Microsoft.AspNetCore.Identity;
using System.Threading;

namespace FaceitParser.Services
{
    public class ServiceResolver : IServiceResolver
    {
        private ISteamApi _steamApi { get; set; }

        public IDictionary<IdentityUser, IDictionary<FaceitService, CancellationTokenSource>> Services { get; set; }


        public ServiceResolver(ISteamApi steamApi)
        {
            _steamApi = steamApi;
            Services = new Dictionary<IdentityUser, IDictionary<FaceitService, CancellationTokenSource>>();
        }


        public async Task<IEnumerable<IFaceitService>> Resolve(IdentityUser user)
        {
            return Services[user].Keys;
        }

        public async Task Create(IdentityUser user, string name, FaceitApi faceitApi, Location location, int delay, int maxLvl, CancellationTokenSource source)
        {
            FaceitService faceitService = new FaceitService(_steamApi, name, location, faceitApi, delay, maxLvl, source.Token);
            await faceitService.Init();
            await faceitService.Start().ConfigureAwait(false);
            if (!Services.ContainsKey(user))
            {
                Services.Add(user, new Dictionary<FaceitService, CancellationTokenSource>());
            }
            Services[user].Add(faceitService, source);
        }

        public async Task Remove(IdentityUser user, string name)
        {
            if (!Services.ContainsKey(user))
            {
                return;
            }
            var service = Services[user].FirstOrDefault(x => x.Key.Name == name);
            if (service.Key == null || service.Value == null)
                return;
            service.Value.Cancel();
            service.Key.Dispose();
            Services[user].Remove(service);
        }


    }
}
