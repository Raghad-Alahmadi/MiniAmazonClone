using System.Data;
using System.Data.SqlClient;
using Dapper;
using MiniAmazonClone.Models;
using Microsoft.Extensions.Configuration;

namespace MiniAmazonClone.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public ProductRepository(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<Product> GetProductById(int productId)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                string sql = "SELECT * FROM Products WHERE ProductId = @ProductId";
                return await db.QueryFirstOrDefaultAsync<Product>(sql, new { ProductId = productId });
            }
        }
    }
}
