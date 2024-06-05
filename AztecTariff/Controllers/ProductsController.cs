using AztecTariff.Data;
using AztecTariffModels.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace Tariff_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : Controller
    {
        private readonly TariffDatabaseContext _dbContext;

        public ProductsController(DbContextOptions<TariffDatabaseContext> options)
        {

        }

        [HttpPost(Name = "CreateProduct")]
        public async Task<ActionResult> Create(Product productDetails)
        {
            if (productDetails != null) 
            {
                _dbContext.Products.Add(productDetails);
                await _dbContext.SaveChangesAsync();
                return CreatedAtAction(nameof(Create), productDetails);
            } else
            {
                return BadRequest();
            }

        }


    }
}
