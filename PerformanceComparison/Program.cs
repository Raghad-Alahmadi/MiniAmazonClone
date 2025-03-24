using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.EntityFrameworkCore;
using MiniAmazonClone.Data;
using MiniAmazonClone.Models;

namespace PerformanceComparison
{
    class Program
    {
        private static readonly string _connectionString = "Server=DESKTOP-QJI3LVT\\SQLEXPRESS;Database=MiniAmazon;Trusted_Connection=True;TrustServerCertificate=True;";
        
        static async Task Main(string[] args)
        {
            Console.WriteLine("======================================");
            Console.WriteLine("   EF CORE VS DAPPER PERFORMANCE TEST  ");
            Console.WriteLine("======================================");
            Console.WriteLine("\nRunning tests...\n");
            
            try
            {
                // STEP 1: Fetch data using EF Core and measure execution time
                Console.WriteLine("Running EF Core test...");
                var efCoreTime = await MeasureEFCorePerformance();
                
                // STEP 2: Fetch data using Dapper and measure execution time
                Console.WriteLine("Running Dapper test...");
                var dapperTime = await MeasureDapperPerformance();
                
                // Create results string
                Console.WriteLine("\n======================================");
                Console.WriteLine("            RESULTS                   ");
                Console.WriteLine("======================================\n");
                
                Console.Write("EF Core execution time: ");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"{efCoreTime.TotalMilliseconds:F2} ms");
                Console.ResetColor();
                
                Console.Write("Dapper execution time: ");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"{dapperTime.TotalMilliseconds:F2} ms");
                Console.ResetColor();
                
                Console.Write("Performance difference: ");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"{efCoreTime.TotalMilliseconds / dapperTime.TotalMilliseconds:F2}x");
                Console.ResetColor();
                
                Console.WriteLine("\nPerformance Analysis:");
                Console.WriteLine("1. Dapper is typically faster because it's a lightweight micro-ORM with minimal overhead");
                Console.WriteLine("2. EF Core provides more features but at the cost of performance");
                Console.WriteLine("3. The difference becomes more pronounced with larger datasets");
                
                Console.WriteLine("\nWhen to use each:");
                Console.WriteLine("- Use EF Core for: Complex domain models, frequent updates, when productivity is more important than raw speed");
                Console.WriteLine("- Use Dapper for: Read-heavy workloads, performance-critical operations, large dataset retrieval");
                

            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                Console.ResetColor();
            }
            
            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
        
        private static async Task<TimeSpan> MeasureEFCorePerformance()
        {
            // Create DbContext options
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer(_connectionString)
                .EnableSensitiveDataLogging(false)
                .Options;
            
            // Create stopwatch to measure time
            var stopwatch = Stopwatch.StartNew();
            
            // Use using statement for proper disposal
            using (var context = new ApplicationDbContext(options))
            {
                // Fetch 50,000 products 
                var products = await context.Products
                    .Take(50000)
                    .ToListAsync();
                
                // Log count for verification
                Console.WriteLine($"EF Core: Retrieved {products.Count} products");
            }
            
            // Stop timing and return elapsed time
            stopwatch.Stop();
            return stopwatch.Elapsed;
        }
        
        private static async Task<TimeSpan> MeasureDapperPerformance()
        {
            // Create stopwatch to measure time
            var stopwatch = Stopwatch.StartNew();
            
            // Use using statement for proper disposal
            using (var connection = new SqlConnection(_connectionString))
            {
                // Open the connection
                await connection.OpenAsync();
                
                // Fetch 50,000 products 
                var query = "SELECT TOP 50000 * FROM Products";
                var products = await connection.QueryAsync<Product>(query);
                
                // Log 
                Console.WriteLine($"Dapper: Retrieved {products.Count()} products");
            }
            
            // Stop timing and return elapsed time
            stopwatch.Stop();
            return stopwatch.Elapsed;
        }
    }
}