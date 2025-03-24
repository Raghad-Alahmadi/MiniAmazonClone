using Microsoft.EntityFrameworkCore;
using MiniAmazonClone.Data;
using MiniAmazonClone.Models;
using MiniAmazonClone.Repositories;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace MiniAmazonClone.Tests.Repositories
{
    public class OrderRepositoryTests
    {
        // REQUIREMENT 1: Mock the database using Moq (5 Marks)
        public interface IDbContextWrapper
        {
            DbSet<Order> Orders { get; }
            int SaveChanges();
        }
        
        public class TestableOrderRepository 
        {
            private readonly IDbContextWrapper _dbWrapper;
            
            public TestableOrderRepository(IDbContextWrapper dbWrapper)
            {
                _dbWrapper = dbWrapper;
            }
            
            public List<Order> GetOrdersByUserId(int userId)
            {
                return _dbWrapper.Orders
                    .Where(o => o.UserId == userId)
                    .ToList();
            }
            
            public Order AddOrder(Order order)
            {
                _dbWrapper.Orders.Add(order);
                _dbWrapper.SaveChanges();
                return order;
            }
        }
        
        // REQUIREMENT 2: Write a unit test for GetOrdersByUserId(int userId) 
        [Fact]
        public void GetOrdersByUserId_ReturnsOrdersForSpecificUser()
        {
            // Arrange
            int userId = 1;
            
            // Create test data
            var orders = new List<Order>
            {
                new Order { OrderId = 1, UserId = userId, OrderDate = DateTime.Now },
                new Order { OrderId = 2, UserId = userId, OrderDate = DateTime.Now.AddDays(-1) },
                new Order { OrderId = 3, UserId = 2, OrderDate = DateTime.Now } 
            }.AsQueryable();
            
            // REQUIREMENT 1 (continued): Mock DbSet for querying
            var mockOrdersDbSet = new Mock<DbSet<Order>>();
            mockOrdersDbSet.As<IQueryable<Order>>().Setup(m => m.Provider).Returns(orders.Provider);
            mockOrdersDbSet.As<IQueryable<Order>>().Setup(m => m.Expression).Returns(orders.Expression);
            mockOrdersDbSet.As<IQueryable<Order>>().Setup(m => m.ElementType).Returns(orders.ElementType);
            mockOrdersDbSet.As<IQueryable<Order>>().Setup(m => m.GetEnumerator()).Returns(() => orders.GetEnumerator());
            
            // REQUIREMENT 1 (continued): Mock the wrapper interface
            var mockDbWrapper = new Mock<IDbContextWrapper>();
            mockDbWrapper.Setup(w => w.Orders).Returns(mockOrdersDbSet.Object);
            
            // Create repository with mocked wrapper
            var repository = new TestableOrderRepository(mockDbWrapper.Object);
            
            // Act
            var result = repository.GetOrdersByUserId(userId);
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.All(result, order => Assert.Equal(userId, order.UserId));
        }
        
        // REQUIREMENT 3: Write a unit test for AddOrder(Order order)
        [Fact]
        public void AddOrder_AddsOrderToDatabase()
        {
            // Arrange
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
            
            // REQUIREMENT 1  Mock DbSet for adding
            var mockOrdersDbSet = new Mock<DbSet<Order>>();
            
            // REQUIREMENT 1  Mock wrapper interface
            var mockDbWrapper = new Mock<IDbContextWrapper>();
            mockDbWrapper.Setup(w => w.Orders).Returns(mockOrdersDbSet.Object);
            
            // Create repository with mocked wrapper
            var repository = new TestableOrderRepository(mockDbWrapper.Object);
            
            // Act
            var result = repository.AddOrder(order);
            
            // Assert - verifying AddOrder works correctly
            mockOrdersDbSet.Verify(m => m.Add(It.IsAny<Order>()), Times.Once());
            mockDbWrapper.Verify(w => w.SaveChanges(), Times.Once());
            Assert.Same(order, result);
        }
    }
}