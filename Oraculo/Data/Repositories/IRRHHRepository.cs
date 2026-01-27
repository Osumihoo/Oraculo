using Oraculo.Models;

namespace Oraculo.Data.Repositories
{
    public interface IRRHHRepository
    {
        Task<List<RRHHVariousDebtors>> GetVariousDebtorsAsync(int environment);
        Task<List<RRHHTimeClock>> GetRRHHTimeClock(int environment);
    }
}
