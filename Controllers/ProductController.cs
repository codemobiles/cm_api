using System;
using System.Linq;
using cm_api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace cm_api.Controllers
{
    [Route("api/[Controller]")]
    [ApiController]
    [Authorize]
    public class ProductController : ControllerBase
    {
        ILogger<ProductController> _logger;
        private readonly DatabaseContext context;

        public ProductController(ILogger<ProductController> logger, DatabaseContext context)
        {
            this.context = context;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult GetProducts()
        {
            try
            {
                var result = context.Products.ToList();
                return Ok(new { result = result, message = "request successfully" });
            }
            catch (Exception error)
            {
                _logger.LogError($"Failed to execute GET {error.ToString()}");
                return StatusCode(500, new { result = "", message = error });
            }
        }

        // localhost:{port}//.../1
        [HttpGet("{id}")]
        public IActionResult GetProduct(int id)
        {
            try
            {
                var result = context.Products.SingleOrDefault(p => p.ProductId == id);

                if (result == null)
                {
                    return NotFound();
                }
                return Ok(new { result = result, message = "request successfully" });
            }
            catch (Exception error)
            {
                _logger.LogError($"Failed to execute GET {error.ToString()}");
                return StatusCode(500, new { result = "", message = error });
            }
        }


        //[AllowAnonymous]
        [HttpGet("images/{name}")]
        public IActionResult GetImage(String name)
        {
            return File($"~/images/{name}", "image/jpg");
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            try
            {
                var item = context.Products.SingleOrDefault(p => p.ProductId == id);

                if (item == null)
                {
                    return NotFound();
                }

                context.Products.Remove(item);
                context.SaveChanges();

                return Ok(new { result = "", message = "delete product sucessfully" });
            }
            catch (Exception error)
            {
                _logger.LogError($"Log DeleteProduct: {error}");
                return StatusCode(500, new { result = "", message = error });
            }
        }


        [HttpGet("count/out_of_stock")]
        public IActionResult GetOutOfStock()
        {
            try
            {
                var count = context.Products.Where(p => p.Stock == 0).Count();
                return Ok(new { out_of_stock_product = count, message = "request successfully" });
            }
            catch (Exception error)
            {
                _logger.LogError($"Log CheckOutOfStock: {error}");
                return StatusCode(500, new { result = "", message = error });
            }
        }        
        
        [HttpGet("search/name")]
        public IActionResult SearchProduct([FromQuery] string keyword)
        {
            try
            {
                var result = (from product in context.Products
                              where EF.Functions.Like(product.Name, "%" + keyword + "%")
                              select product).ToList();

                return Ok(new { result = result, message = "request successfully" });
            }
            catch (Exception error)
            {
                _logger.LogError($"Log SearchProducts: {error}");
                return StatusCode(500, new { result = "", message = error });
            }
        }

    }
}