using FaceitParser.Models;
using FaceitParser.Services;
using Microsoft.AspNetCore.Identity;

namespace FaceitParser.Abstractions
{
    public interface IServiceResolver
    {
        /// <summary>
        /// Получает инстансы сервиса FaceitService для текущего пользователя
        /// </summary>
        /// <param name="user"></param>
        /// <param name="name"></param>
        /// <returns>
        /// Массив из сервисов FaceitService для указанного пользователя
        /// </returns>
        IEnumerable<IFaceitService> Resolve(string user, string name = null);

        /// <summary>
        /// Создает и добавляет новый инстанс фейсит парсера в пул
        /// </summary>
        /// <param name="user"></param>
        /// <param name="name"></param>
        /// <param name="faceitApi"></param>
        /// <param name="location"></param>
        /// <param name="delay"></param>
        /// <param name="maxLvl"></param>
        /// <param name="source"></param>
        Task Create(string user, string name, FaceitApi faceitApi, Location location, int delay, int maxLvl, int maxMatches, int minPrice, CancellationTokenSource source);

        /// <summary>
        /// Отменяет и удаляет инстанс сервиса по названию
        /// </summary>
        /// <param name="user"></param>
        /// <param name="name"></param>
        void Remove(string user, string name);
    }
}
