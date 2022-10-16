using Microsoft.AspNetCore.Identity;

namespace FaceitParser.Abstractions
{
    public interface IServiceResolver
    {
        /// <summary>
        /// Получает все инстансы сервиса FaceitService для текущего пользователя
        /// </summary>
        /// <param name="user"></param>
        /// <returns>
        /// Массив из сервисов FaceitService для указанного пользователя
        /// </returns>
        Task<IEnumerable<IFaceitService>> Resolve(IdentityUser user);


    }
}
