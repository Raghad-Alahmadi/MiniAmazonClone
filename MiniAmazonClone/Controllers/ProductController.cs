using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniAmazonClone.Repositories;
using MiniAmazonClone.Models;
using MiniAmazonClone.Data;

namespace MiniAmazonClone.Controllers
{
    [Route("api/products")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductRepository _productRepository;
        private readonly ApplicationDbContext _context;

        public ProductController(IProductRepository productRepository, ApplicationDbContext context)
        {
            _productRepository = productRepository;
            _context = context;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult AddProduct([FromBody] Product product)
        {
            _context.Products.Add(product);
            _context.SaveChanges();
            return Ok("Product added successfully.");
        }

        [HttpGet]
        public IActionResult GetAllProducts()
        {
            var products = _context.Products.ToList();
            return Ok(products);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductById(int id)
        {
            var product = await _productRepository.GetProductById(id);
            if (product == null)
            {
                return NotFound("Product not found.");
            }

            return Ok(product);
        }
    }
}
