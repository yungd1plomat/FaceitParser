namespace FaceitParser.Services
{
    public class ServiceResolver
    {
        private SteamApi _steamApi { get; set; }

        public ServiceResolver(SteamApi steamApi)
        {
            _steamApi = steamApi;
        }
    }
}
