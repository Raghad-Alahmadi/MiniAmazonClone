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

                foreach (var item in order.OrderItems)
                {
                    var product = _context.Products.FirstOrDefault(p => p.ProductId == item.ProductId);

                    if (product == null)
                    {
                        return BadRequest($"Product with ID {item.ProductId} not found.");
                    }

                    item.Price = product.Price;

                    totalAmount += item.Price * item.Quantity;

                    product.Stock -= item.Quantity;

                    if (product.Stock < 0)
                    {
                        return BadRequest($"Not enough stock for product: {product.Name}");
                    }
                }

                order.TotalAmount = totalAmount;

                _context.Orders.Add(order);
                _context.SaveChanges();

                return Ok("Order placed successfully.");
            }
            catch (Exception ex)
            {
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


        [HttpPost("refund/{id}")]
        [Authorize(Policy = "CanRefundOrders")]
        public IActionResult RefundOrder(int id)
        {
            var order = _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefault(o => o.OrderId == id);

            if (order == null)
            {
                return NotFound("Order not found.");
            }

            order.Status = "Refunded";
            _context.SaveChanges();

            return Ok("Order refunded successfully.");
        }
    }
}
