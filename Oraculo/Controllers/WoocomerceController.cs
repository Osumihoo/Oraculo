using Microsoft.AspNetCore.Mvc;
using Oraculo.Data.Repositories;
using Oraculo.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Oraculo.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class WoocomerceController : ControllerBase
    {
        private readonly IWoocommerceRepository _woocommerceRepository;

        public WoocomerceController(IWoocommerceRepository woocommerceRepository)
        {
            _woocommerceRepository = woocommerceRepository;
        }
        // GET: api/<WoocomerceController>
        [HttpGet("{environment}")]
        public async Task<IActionResult> GetWoocommerceStockComplete(int environment)
        {
            try
            {
                var stockList = await _woocommerceRepository.GetStockComplete(environment);

                var response = new Response<List<WoocommerceStockComplete>>
                {
                    Code = 200,
                    Description = "Consulta exitosa",
                    Data = stockList
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

        // GET: api/Woocomerce/{environment}/PriceUpdates
        [HttpGet("{environment}/PriceUpdates")]
        public async Task<IActionResult> GetWoocommercePriceUpdates(int environment)
        {
            try
            {
                var priceUpdates = await _woocommerceRepository.GetPriceUpdates(environment);

                var response = new Response<List<WoocommercePriceUpdate>>
                {
                    Code = 200,
                    Description = "Consulta de actualizaciones de precios exitosa",
                    Data = priceUpdates
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                var error = new Response<string>
                {
                    Code = 500,
                    Description = "Error al obtener las actualizaciones de precios: " + ex.Message,
                    Data = null
                };

                return StatusCode(500, error);
            }
        }

        [HttpGet("{environment}/{branch}")]
        public async Task<IActionResult> SAPGetStockbybranch(int environment, string branch)
        {
            if (string.IsNullOrWhiteSpace(branch))
            {
                return BadRequest(new Response
                {
                    Code = 400,
                    Description = "El campo 'Branch' es obligatorio."
                });
            }

            try
            {
                var stockList = await _woocommerceRepository.GetAllStockByBranch(environment, branch);

                var response = new Response<List<WoocommerceStockComplete>>
                {
                    Code = 200,
                    Description = "Consulta exitosa",
                    Data = stockList
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

        [HttpGet("{environment}/{branch}/{item}")]
        public async Task<IActionResult> SAPGetStockbybranchitem(int environment, string branch, string item)
        {
            if (string.IsNullOrWhiteSpace(branch) || string.IsNullOrWhiteSpace(item))
            {
                return BadRequest(new Response
                {
                    Code = 400,
                    Description = "El campo 'Branch' y 'Item' son obligatorios."
                });
            }

            try
            {
                var stockList = await _woocommerceRepository.GetAllStockByBranchByProduct(environment, branch, item);

                var response = new Response<WoocommerceStockComplete>
                {
                    Code = 200,
                    Description = "Consulta exitosa",
                    Data = stockList
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
