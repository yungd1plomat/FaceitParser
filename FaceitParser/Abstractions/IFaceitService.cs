namespace FaceitParser.Abstractions
{
    public interface IFaceitService
    {
        /// <summary>
        /// Инициализирует новый инстанс сервиса
        /// </summary>
        Task Init();

        /// <summary>
        /// Запускает парсинг игроков с матчей и добавляет их в друзья
        /// </summary>
        Task Start();

        /// <summary>
        /// Парсит игроков матчей с указанными в конструкторе параметрами
        /// </summary>
        /// <returns></returns>
        Task LoopGames();

        /// <summary>
        /// Добавляет игроков в друзья по 100 человек из очереди
        /// </summary>
        Task LoopPlayers();
    }
}
