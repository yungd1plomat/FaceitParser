using FaceitParser.Models;
using FaceitParser.Models.Steam;

namespace FaceitParser.Abstractions
{
    public interface ISteamApi
    {
        /// <summary>
        /// Получает инвентарь указанного профиля
        /// </summary>
        /// <param name="steamID">STEAMID64 профиля</param>
        /// <returns>Инвентарь</returns>
        Task<Inventory> GetInventory(ulong steamID);

        /// <summary>
        /// Получает все существующие лоты на тп и их цену
        /// </summary>
        /// <returns>Средняя цена каждого предмета на тп в долларах</returns>
        Task<Dictionary<string, double>> GetItems();
    }
}
