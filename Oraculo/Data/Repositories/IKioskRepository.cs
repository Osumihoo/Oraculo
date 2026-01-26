using Oraculo.Models;

namespace Oraculo.Data.Repositories
{
    public interface IKioskRepository
    {
        Task<List<KioskStockComplete>> GetKioskStockComplete(int environment, string whsCode, string priceList);
        Task<List<KioskPriceUpdates>> GetKioskPriceUpdates(int environment, string whsCode, string priceList);
    }
}
