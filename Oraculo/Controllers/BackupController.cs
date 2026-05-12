using Microsoft.AspNetCore.Mvc;
using Oraculo.Data.Repositories;
using Oraculo.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Oraculo.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class BackupController : ControllerBase
    {
        private readonly IBackupRepository _backupRepository;

        public BackupController(IBackupRepository backupRepository)
        {
            _backupRepository = backupRepository;
        }

        [HttpGet("BackupCostosPreciosExistencias/{environment}")]
        public async Task<IActionResult> ObtenerBackupCostosPreciosExistencias(int environment)
        {
            try
            {
                var resultado = await _backupRepository.ObtenerBackupCostosPreciosExistenciasAsync(environment);

                var response = new Response<List<BackupCostosPreciosExistencias>>
                {
                    Code = 200,
                    Description = "Consulta exitosa",
                    Data = resultado
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                var error = new Response<string>
                {
                    Code = 500,
                    Description = "Error al obtener los datos: " + ex.Message,
                    Data = null
                };

                return StatusCode(500, error);
            }
        }

        [HttpGet("BackupSaldosDeudoresDiversos/{environment}")]
        public async Task<IActionResult> ObtenerBackupSaldosDeudoresDiversos(int environment)
        {
            try
            {
                var resultado = await _backupRepository.ObtenerBackupSaldosDeudoresDiversosAsync(environment);

                var response = new Response<List<BackupSaldosDeudoresDiversos>>
                {
                    Code = 200,
                    Description = "Consulta exitosa",
                    Data = resultado
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                var error = new Response<string>
                {
                    Code = 500,
                    Description = "Error al obtener los datos: " + ex.Message,
                    Data = null
                };

                return StatusCode(500, error);
            }
        }

        [HttpGet("BackupSaldosSociosDeNegocios/{environment}")]
        public async Task<IActionResult> ObtenerBackupSaldosSociosDeNegocios(int environment)
        {
            try
            {
                var resultado = await _backupRepository.ObtenerBackupSaldosSociosDeNegociosAsync(environment);

                var response = new Response<List<BackupSaldosSociosDeNegocios>>
                {
                    Code = 200,
                    Description = "Consulta exitosa",
                    Data = resultado
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                var error = new Response<string>
                {
                    Code = 500,
                    Description = "Error al obtener los datos: " + ex.Message,
                    Data = null
                };

                return StatusCode(500, error);
            }
        }

        [HttpGet("BackupStock/{environment}")]
        public async Task<IActionResult> ObtenerBackupStock(int environment)
        {
            try
            {
                var resultado = await _backupRepository.ObtenerBackupStockAsync(environment);

                var response = new Response<List<BackupStock>>
                {
                    Code = 200,
                    Description = "Consulta exitosa",
                    Data = resultado
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                var error = new Response<string>
                {
                    Code = 500,
                    Description = "Error al obtener los datos: " + ex.Message,
                    Data = null
                };

                return StatusCode(500, error);
            }
        }
    }
}
