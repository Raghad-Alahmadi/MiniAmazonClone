using System.Data;
using System.Data.SqlClient;
using Dapper;
using MiniAmazonClone.Models;
using Microsoft.Extensions.Configuration;

namespace MiniAmazonClone.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public OrderRepository(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("DefaultConnection");
        }
        protected virtual IDbConnection CreateConnection()
        {
            return new SqlConnection(_connectionString);
        }
        public async Task<IEnumerable<Order>> GetCustomerOrders(int userId)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                string sql = @"
                    SELECT * FROM Orders o
                    INNER JOIN OrderItems oi ON o.OrderId = oi.OrderId
                    WHERE o.UserId = @UserId";

                var orderDictionary = new Dictionary<int, Order>();

                var orders = await db.QueryAsync<Order, OrderItem, Order>(
                    sql,
                    (order, orderItem) =>
                    {
                        if (!orderDictionary.TryGetValue(order.OrderId, out var currentOrder))
                        {
                            currentOrder = order;
                            currentOrder.OrderItems = new List<OrderItem>();
                            orderDictionary.Add(currentOrder.OrderId, currentOrder);
                        }

                        currentOrder.OrderItems.Add(orderItem);
                        return currentOrder;
                    },
                    new { UserId = userId },
                    splitOn: "OrderId,OrderItemId"
                );

                return orders.Distinct().ToList();
            }
        }
    }
}
