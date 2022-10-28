﻿using FaceitParser.Abstractions;
using FaceitParser.Data;
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

        public IDictionary<string, IDictionary<FaceitService, CancellationTokenSource>> Services { get; set; }

        

        public ServiceResolver(ISteamApi steamApi, IConfiguration configuration)
        {
            _configuration = configuration;
            _steamApi = steamApi;
            Services = new Dictionary<string, IDictionary<FaceitService, CancellationTokenSource>>();
        }


        public IEnumerable<FaceitService> Resolve(string user, string name = null)
        {
            if (!Services.ContainsKey(user))
                return Array.Empty<FaceitService>();
            if (name is null)
                return Services[user].Keys;
            return Services[user].Keys.Where(x => x.Name == name);
        }

        public async Task Create(string user, string name, FaceitApi faceitApi, Location location, int delay, int maxLvl, int minPrice, CancellationTokenSource source)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseMySql(_configuration["CONNECTION_STRING"], new MySqlServerVersion(new Version(8, 0, 27)));
            // Для использования в разных потоках (чтобы не было конфликтов)
            var playersDb = new ApplicationDbContext(optionsBuilder.Options);
            var friendsDb = new ApplicationDbContext(optionsBuilder.Options);
            FaceitService faceitService = new FaceitService(_steamApi, name, location, faceitApi, delay, maxLvl, minPrice, playersDb, friendsDb, user, source.Token);
            await faceitService.Init();
            await faceitService.Start().ConfigureAwait(false);
            if (!Services.ContainsKey(user))
            {
                Services.Add(user, new Dictionary<FaceitService, CancellationTokenSource>());
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
            service.Value.Cancel();
            service.Key.Dispose();
            Services[user].Remove(service);
        }


    }
}
