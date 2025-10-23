using Microsoft.AspNetCore.Mvc;
using Oraculo.Data.Repositories;
using Oraculo.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Oraculo.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class BranchManagersController : ControllerBase
    {
        private readonly IBranchManagersRepository _branchManagersrepository;

        public BranchManagersController(IBranchManagersRepository branchManagersrepository)
        {
            _branchManagersrepository = branchManagersrepository;
        }

        [HttpGet("Restocked/{environment}/{grupo}/{sucursal}")]
        public async Task<IActionResult> GetStockResupply(int environment, string grupo, string sucursal)
        {
            try
            {
                var resupply = await _branchManagersrepository.GetStockResupply(environment, grupo, sucursal);

                var response = new Response<List<Dictionary<string, object>>>
                {
                    Code = 200,
                    Description = "Consulta exitosa",
                    Data = resupply
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

        [HttpGet("ZeroRestocked/{environment}/{grupo}")]
        public async Task<IActionResult> GetStockZeroResupply(int environment, string grupo)
        {
            try
            {
                var data = await _branchManagersrepository.GetStockZeroResupply(environment, grupo);

                var response = new Response<List<Dictionary<string, object>>>
                {
                    Code = 200,
                    Description = "Consulta exitosa",
                    Data = data
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

        [HttpGet("Last45Days/{environment}/{family}")]
        public async Task<IActionResult> GetLast45DaysByFamily(int environment, string family)
        {
            try
            {
                var data = await _branchManagersrepository.GetLast45DaysByFamily(environment, family);

                var response = new Response<List<Dictionary<string, object>>>
                {
                    Code = 200,
                    Description = "Consulta exitosa",
                    Data = data
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
