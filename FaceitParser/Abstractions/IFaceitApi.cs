using FaceitParser.Models;
using System.Numerics;

namespace FaceitParser.Abstractions
{
    /// <summary>
    /// Имплементация фейсит апи
    /// </summary>
    public interface IFaceitApi
    {
        /// <summary>
        /// Ник авторизованного пользователя
        /// </summary>
        string SelfNick { get; }

        /// <summary>
        /// Токен авторизованного пользователя
        /// </summary>
        string Token { get; }

        /// <summary>
        /// Инициализирует API клиент с заданным API ключом
        /// и при желании прокси
        /// </summary>
        /// <param name="apiKey">Bearer апи ключ</param>
        /// <param name="proxy">Прокси</param>
        /// <param name="proxyType">Тип прокси (https, socks4, socks5)</param>
        void Init(string apiKey, string proxy, string proxyType);

        /// <summary>
        /// Получает все игры в лайве фейсита
        /// </summary>
        /// <param name="region">Регион поиска</param>
        /// <param name="offset">Смещение относительно начала игр</param>
        /// <param name="limit">Максимальное количество игр, которые нужно вернуть (максимум 100)</param>
        /// <returns>
        /// Id матчей идущих в лайве
        /// </returns>
        Task<IEnumerable<string>> GetGameIdsAsync(string regionId, int offset = 0, int limit = 100);

        /// <summary>
        /// Получает всех игроков указанного матча, 
        /// лвлы которых больше указанного
        /// </summary>
        /// <param name="matchId">Id матча</param>
        /// <param name="maxLevel">Максимальный левел игрока</param>
        /// <returns>
        /// Массив пользователей (без указания страны)
        /// </returns>
        Task<IEnumerable<Player>> GetPlayersAsync(string matchId, int maxLevel = 11);

        /// <summary>
        /// Получает страны указанных пользователей
        /// </summary>
        /// <param name="players">Массив пользователей (без указанной страны)</param>
        /// <param name="countries">Пользователей каких стран нужно оставить (передать null, если брать всех пользователей) </param>
        /// <param name="ignoreCountries">Пользователей каких стран нужно проигнорировать (передать null, если не игнорировать)</param>
        /// <returns>
        /// Отфильтрованный массив пользователей (с указанием страны)
        /// </returns>
        Task<IEnumerable<Player>> GetPlayersAsync(IEnumerable<Player> players, IEnumerable<string> countries, IEnumerable<string> ignoreCountries);

        /// <summary>
        /// Получает данные о текущем авторизованном пользователе
        /// </summary>
        Task GetSelf();

        /// <summary>
        /// Отправляет заявку в друзья указанным пользователям
        /// Максимум можно указать 99 игроков для 1 запроса
        /// </summary>
        /// <param name="players">Игроки, которым нужно отправить заявку</param>
        Task AddFriendsAsync(IEnumerable<Player> players);
    }
}
