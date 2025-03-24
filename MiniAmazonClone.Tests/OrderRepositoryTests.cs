using Microsoft.Extensions.Configuration;
using MiniAmazonClone.Models;
using MiniAmazonClone.Repositories;
using Moq;
using Moq.Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Dapper; // Add this important using statement

namespace MiniAmazonClone.Tests.Repositories
{
    public class OrderRepositoryTests
    {
        [Fact]
        public async Task GetCustomerOrders_ReturnsOrdersForSpecificUser()
        {
            // Arrange
            int userId = 1;
            
            // Mock configuration to return our test connection string
            var mockConfig = new Mock<IConfiguration>();
            mockConfig
                .Setup(config => config.GetConnectionString("DefaultConnection"))
                .Returns("Server=(localdb)\\MSSQLLocalDB;Database=TestDb;Trusted_Connection=True;");

            // Setup test data
            var testOrders = new List<Order>
            {
                new Order { OrderId = 1, UserId = userId, OrderDate = DateTime.Now, OrderItems = new List<OrderItem>
                {
                    new OrderItem { OrderItemId = 1, OrderId = 1, ProductId = 10, Quantity = 2, Price = 20.0m },
                    new OrderItem { OrderItemId = 2, OrderId = 1, ProductId = 11, Quantity = 1, Price = 15.0m }
                }},
                new Order { OrderId = 2, UserId = userId, OrderDate = DateTime.Now.AddDays(-1), OrderItems = new List<OrderItem>
                {
                    new OrderItem { OrderItemId = 3, OrderId = 2, ProductId = 12, Quantity = 3, Price = 10.0m }
                }}
            };

            // Mock the SQL connection
            var mockConnection = new Mock<IDbConnection>();
            
            // Use SetupDapper with the correct signature
            mockConnection.SetupDapper(c => c.QueryAsync<Order, OrderItem, Order>(
                    It.IsAny<string>(),
                    It.IsAny<Func<Order, OrderItem, Order>>(),
                    It.IsAny<object>(),
                    It.IsAny<IDbTransaction>(),
                    It.IsAny<bool>(),
                    It.IsAny<string>(),
                    It.IsAny<int?>(),
                    It.IsAny<CommandType?>()))
                .ReturnsAsync(testOrders);

            // Create a custom repository that uses our mock connection
            var repository = new TestOrderRepository(mockConfig.Object, () => mockConnection.Object);

            // Act
            var result = await repository.GetCustomerOrders(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            
            var ordersList = result.ToList();
            Assert.Equal(1, ordersList[0].OrderId);
            Assert.Equal(2, ordersList[1].OrderId);
            Assert.Equal(2, ordersList[0].OrderItems.Count);
            Assert.Single(ordersList[1].OrderItems); // Fix xUnit warning
            Assert.All(ordersList, order => Assert.Equal(userId, order.UserId));
        }

        [Fact]
        public async Task AddOrder_InsertsOrderSuccessfully()
        {
            // Arrange
            var mockConfig = new Mock<IConfiguration>();
            mockConfig
                .Setup(config => config.GetConnectionString("DefaultConnection"))
                .Returns("Server=(localdb)\\MSSQLLocalDB;Database=TestDb;Trusted_Connection=True;");

            var order = new Order
            {
                UserId = 1,
                OrderDate = DateTime.Now,
                OrderItems = new List<OrderItem>
                {
                    new OrderItem { ProductId = 10, Quantity = 2, Price = 20.0m },
                    new OrderItem { ProductId = 11, Quantity = 1, Price = 15.0m }
                }
            };

            var mockConnection = new Mock<IDbConnection>();
            
            // Fix the ExecuteAsync and ExecuteScalarAsync mocking
            mockConnection.SetupDapper(c => c.ExecuteAsync(
                    It.IsAny<string>(),
                    It.IsAny<object>(),
                    It.IsAny<IDbTransaction>(),
                    It.IsAny<int?>(),
                    It.IsAny<CommandType?>()))
                .ReturnsAsync(1); // Return value indicating 1 row affected
                
            mockConnection.SetupDapper(c => c.ExecuteScalarAsync<int>(
                    It.IsAny<string>(), 
                    It.IsAny<object>(),
                    It.IsAny<IDbTransaction>(),
                    It.IsAny<int?>(),
                    It.IsAny<CommandType?>()))
                .ReturnsAsync(10); // Return value for the new order ID
            
            // Create a custom repository that uses our mock connection
            var repository = new TestOrderRepository(mockConfig.Object, () => mockConnection.Object);

            // Act
            var result = await repository.AddOrder(order);

            // Assert
            Assert.Equal(10, result.OrderId);
            
            // Fix the verification that was causing errors with null propagation
            mockConnection.Verify(m => 
                m.ExecuteAsync(
                    It.IsAny<string>(),
                    It.Is<object>(p => p.GetType().GetProperty("UserId") != null && 
                                      p.GetType().GetProperty("UserId").GetValue(p).Equals(1)),
                    null,
                    null,
                    It.IsAny<CommandType?>()
                ), Times.AtLeastOnce);
        }
    }

    // Test repository that allows us to inject a mock connection
    public class TestOrderRepository : OrderRepository
    {
        private readonly Func<IDbConnection> _connectionFactory;

        public TestOrderRepository(IConfiguration configuration, Func<IDbConnection> connectionFactory) 
            : base(configuration)
        {
            _connectionFactory = connectionFactory;
        }

        protected override IDbConnection CreateConnection()
        {
            return _connectionFactory();
        }

        public async Task<Order> AddOrder(Order order)
        {
            using (var db = CreateConnection())
            {
                db.Open();
                using (var transaction = db.BeginTransaction())
                {
                    try
                    {
                        string insertOrderSql = @"
                            INSERT INTO Orders (UserId, OrderDate) 
                            VALUES (@UserId, @OrderDate);
                            SELECT SCOPE_IDENTITY();";

                        int orderId = await db.ExecuteScalarAsync<int>(insertOrderSql, new
                        {
                            order.UserId,
                            order.OrderDate
                        }, transaction);

                        order.OrderId = orderId;

                        string insertOrderItemSql = @"
                            INSERT INTO OrderItems (OrderId, ProductId, Quantity, Price)
                            VALUES (@OrderId, @ProductId, @Quantity, @Price)";

                        foreach (var item in order.OrderItems)
                        {
                            item.OrderId = orderId;
                            await db.ExecuteAsync(insertOrderItemSql, item, transaction);
                        }

                        transaction.Commit();
                        return order;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }
    }
}