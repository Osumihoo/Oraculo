using Oraculo.Models;

namespace Oraculo.Data.Repositories
{
    public interface IBackupRepository
    {
        Task<List<BackupCostosPreciosExistencias>> ObtenerBackupCostosPreciosExistenciasAsync(int environment);
        Task<List<BackupSaldosDeudoresDiversos>> ObtenerBackupSaldosDeudoresDiversosAsync(int environment);
        Task<List<BackupSaldosSociosDeNegocios>> ObtenerBackupSaldosSociosDeNegociosAsync(int environment);
        Task<List<BackupStock>> ObtenerBackupStockAsync(int environment);
    }
}
