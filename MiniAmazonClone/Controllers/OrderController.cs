using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniAmazonClone.Repositories;
using MiniAmazonClone.Models;
using MiniAmazonClone.Data;
using Microsoft.EntityFrameworkCore;

namespace MiniAmazonClone.Controllers
{
    [Route("api/orders")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ApplicationDbContext _context;

        public OrderController(IOrderRepository orderRepository, ApplicationDbContext context)
        {
            _orderRepository = orderRepository;
            _context = context;
        }

        [HttpPost("create")]
        [Authorize(Roles = "Customer")]
        public IActionResult CreateOrder([FromBody] Order order)
        {
            try
            {
                // Extract the UserId from the JWT token
                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    return Unauthorized("User ID not found in token.");
                }

                order.UserId = int.Parse(userIdClaim.Value);

                if (order.OrderItems == null || !order.OrderItems.Any())
                {
                    return BadRequest("Order must contain at least one OrderItem.");
                }

                decimal totalAmount = 0;

                // Loop through OrderItems to associate products and calculate the total
                foreach (var item in order.OrderItems)
                {
                    var product = _context.Products.FirstOrDefault(p => p.ProductId == item.ProductId);

                    // If the product is not found, return a bad request
                    if (product == null)
                    {
                        return BadRequest($"Product with ID {item.ProductId} not found.");
                    }

                    item.Price = product.Price;

                    // Calculate the total amount for the order
                    totalAmount += item.Price * item.Quantity;

                    // Update the product stock 
                    product.Stock -= item.Quantity;

                    // Ensure product stock doesn't go negative
                    if (product.Stock < 0)
                    {
                        return BadRequest($"Not enough stock for product: {product.Name}");
                    }
                }

                // Set the TotalAmount for the order
                order.TotalAmount = totalAmount;

                // Add the order to the database
                _context.Orders.Add(order);
                _context.SaveChanges();

                // Return a success response
                return Ok("Order placed successfully.");
            }
            catch (Exception ex)
            {
                // Log any exception
                Console.WriteLine($"Error creating order: {ex.Message}");
                return StatusCode(500, "An error occurred while creating the order.");
            }
        }

        [HttpGet("customer")]
        [Authorize]
        public async Task<IActionResult> GetCustomerOrders()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized("User ID not found in token.");
            }

            var userId = int.Parse(userIdClaim.Value);
            var orders = await _orderRepository.GetCustomerOrders(userId);
            return Ok(orders);
        }

        [HttpGet("{id}")]
        [Authorize]
        public IActionResult GetOrder(int id)
        {
            var order = _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefault(o => o.OrderId == id);

            if (order == null)
            {
                return NotFound("Order not found.");
            }

            return Ok(order);
        }

        [HttpGet("all")]
        [Authorize(Policy = "CanViewOrders")]
        public IActionResult GetAllOrders()
        {
            var orders = _context.Orders
                .Include(o => o.OrderItems)
                .ToList();

            return Ok(orders);
        }
    }
}
