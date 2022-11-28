using FaceitParser.Helpers.Extensions;
using FaceitParser.Models;
using FaceitParser.Models.App;
using FaceitParser.Models.Faceit;
using System.Collections.Concurrent;

namespace FaceitParser.Abstractions
{
    public interface IFaceitService
    {
        /// <summary>
        /// <summary>
        /// Название текущего парсера
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Задержка для запросов
        /// </summary>
        int Delay { get; }

        /// <summary>
        /// Общее количество просмотренных игр
        /// </summary>
        ConcurrentInt Games { get; }

        /// <summary>
        /// Общее количество просмотренных игроков
        /// </summary>
        ConcurrentInt Total { get; }

        /// <summary>
        /// Количество спаршенных игроков
        /// </summary>
        ConcurrentInt Parsed { get; }

        /// <summary>
        /// Количество добавленных игроков
        /// </summary>
        ConcurrentInt Added { get; }

        /// <summary>
        /// Настройки локации текущего парсера
        /// </summary>
        Location Location { get; }

        /// <summary>
        /// Ник авторизованного аккаунта
        /// </summary>
        string AccountNick { get; }

        /// <summary>
        /// Содержит необработанные логи
        /// </summary>
        ConcurrentQueue<string> Logs { get; }

        /// <summary>
        /// Содержит спаршенные SteamId64
        /// </summary>
        ConcurrentQueue<string> SteamIds { get; set; }

        /// <summary>
        /// Инициализирует новый инстанс сервиса
        /// </summary>
        Task Init();

        /// <summary>
        /// Запускает парсинг игроков с матчей и добавляет их в друзья
        /// </summary>
        Task Start();

        /// <summary>
        /// Ассинхронно обрабатывает матч по Id
        /// </summary>
        Task ProcessGame(string gameId);

        /// <summary>
        /// Добавляет игроков в друзья из очереди
        /// </summary>
        Task LoopPlayers();

        /// <summary>
        /// Добавляет игроков в blacklist
        /// </summary>
        /// <returns></returns>
        Task LoopBlacklist();

        /// <summary>
        /// Получает стоимость инвентаря игрока
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        Task<double> GetInventoryPrice(Player player);
    }
}
