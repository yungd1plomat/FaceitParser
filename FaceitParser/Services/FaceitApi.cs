using FaceitParser.Abstractions;
using FaceitParser.Models;
using System.Net.Http.Headers;
using System.Net;
using System.Text.Json.Nodes;
using System.Text.Json;
using System.Text;

namespace FaceitParser.Services
{
    public class FaceitApi : IFaceitApi, IDisposable
    {
        public string SelfNick { get; set; }

        private const string baseUrl = "https://api.faceit.com";

        private HttpClient client { get; set; }

        private string selfId { get; set; }

        private CancellationToken cancellationToken { get; set; }

        public FaceitApi(string apiKey, CancellationToken cancellationToken, string proxy = null, string proxyType = null)
        {
            Init(apiKey, proxy, proxyType);
            this.cancellationToken = cancellationToken;
        }

        /// <inheritdoc/>
        public void Init(string apiKey, string proxy, string proxyType)
        {
            if (proxy is not null)
            {
                var proxyData = proxy.Split(':', StringSplitOptions.RemoveEmptyEntries);
                var webProxy = new WebProxy();
                webProxy.Address = new Uri($"{proxyType}//{proxyData[0]}:{proxyData[1]}");
                if (proxyData.Length > 2)
                {
                    webProxy.Credentials = new NetworkCredential()
                    {
                        UserName = proxyData[2],
                        Password = proxyData[3],
                    };
                }
                var handler = new HttpClientHandler
                {
                    Proxy = webProxy
                };
                client = new HttpClient(handler);
                return;
            }
            client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            client.Timeout = TimeSpan.FromSeconds(10);
        }

        /// <inheritdoc/>
        public async Task GetSelf()
        {
            var response = await client.GetAsync($"{baseUrl}/post/v2/profiles", cancellationToken);
            response.EnsureSuccessStatusCode();
            var stream = await response.Content.ReadAsStreamAsync();
            var content = await JsonSerializer.DeserializeAsync<JsonNode>(stream);
            selfId = content["results"][0]["id"].ToString();

            await Task.Delay(300);

            response = await client.GetAsync($"{baseUrl}/post/v1/users/{selfId}", cancellationToken);
            response.EnsureSuccessStatusCode();
            stream = await response.Content.ReadAsStreamAsync();
            content = await JsonSerializer.DeserializeAsync<JsonNode>(stream);
            SelfNick = content["nickname"].ToString();
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<string>> GetGameIdsAsync(string region, int offset = 0, int limit = 100)
        {
            var response = await client.GetAsync($"{baseUrl}/match/v1/matches/list?game=csgo&" +
                                                                                  $"region={region}&" +
                                                                                  $"state=SUBSTITUTION&" +
                                                                                  $"state=CAPTAIN_PICK&" +
                                                                                  $"state=VOTING&" +
                                                                                  $"state=CONFIGURING&" +
                                                                                  $"state=READY&" +
                                                                                  $"state=ONGOING&" +
                                                                                  $"state=MANUAL_RESULT&" +
                                                                                  $"state=PAUSED&state=ABORTED&" +
                                                                                  $"limit={limit}&" +
                                                                                  $"entityType=matchmaking&" +
                                                                                  $"offset={offset}", cancellationToken);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStreamAsync();
            var matches = await JsonSerializer.DeserializeAsync<Matches>(json);

            return matches?.Games?.Select(x => x.Id);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Player>> GetPlayersAsync(string matchId, int maxLevel = 11)
        {
            var response = await client.GetAsync($"{baseUrl}/match/v2/match/{matchId}", cancellationToken);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStreamAsync();
            var matchInfo = await JsonSerializer.DeserializeAsync<MatchInfo>(json);

            var allPlayers = matchInfo?.Match?.Teams?.Faction1?.Players?.Concat(matchInfo?.Match?.Teams?.Faction2?.Players);
            return allPlayers?.Where(x => x.Level < maxLevel);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Player>> GetPlayersAsync(IEnumerable<Player> players, IEnumerable<string> countries = null, IEnumerable<string> ignoreCountries = null)
        {
            var ids = players.Select(x => x.Id);
            var playerIds = new PlayerIds() { Ids = ids };
            var playersJson = JsonSerializer.Serialize(playerIds);

            var response = await client.PostAsync($"{baseUrl}/user-summary/v1/list", new StringContent(playersJson, Encoding.UTF8, "application/json"), cancellationToken);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStreamAsync();

            var playersInfo = await JsonSerializer.DeserializeAsync<PlayerInfo>(json);
            var onlyPlayers = playersInfo.Players.Select(x => x.Value);
            foreach (var player in onlyPlayers)
            {
                player.ProfileId = players.FirstOrDefault(x => x.Id == player.Id).ProfileId;
                player.Level = players.FirstOrDefault(x => x.Id == player.Id).Level;
                player.Nick = players.FirstOrDefault(x => x.Id == player.Id).Nick;
            }
            var filteredPlayers = countries is null ? onlyPlayers : onlyPlayers.Where(x => countries.Contains(x.Country));
            var ignoredPlayers = ignoreCountries is null ? filteredPlayers : filteredPlayers.Where(x => !ignoreCountries.Contains(x.Country));

            return ignoredPlayers;
        }

        /// <inheritdoc/>
        public async Task AddFriendsAsync(IEnumerable<Player> players)
        {
            if (players.Count() > 99)
                throw new Exception("99 is the maximum players to add friends");
            var ids = players.Select(x => x.Id).ToList();
            FriendsRequest friendsRequest = new FriendsRequest() { Users = ids };
            var json = JsonSerializer.Serialize(friendsRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            if (selfId is null)
                throw new Exception("Self Id empty, initialize it");
            var resp = await client.PostAsync($"{baseUrl}/friend-requests/v1/users/{selfId}/requests", content, cancellationToken);
            resp.EnsureSuccessStatusCode();
        }


        public void Dispose()
        {
            client?.Dispose();
        }
    }
}
