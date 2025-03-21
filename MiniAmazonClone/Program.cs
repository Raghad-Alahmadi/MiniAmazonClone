using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MiniAmazonClone.Data;
using MiniAmazonClone.Services;
using MiniAmazonClone.Repositories;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Database Connection
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// JWT Service Configuration
var jwtSettings = builder.Configuration.GetSection("Jwt");
string secretKey = jwtSettings["Key"];
string issuer = jwtSettings["Issuer"];
string audience = jwtSettings["Audience"];

builder.Services.AddSingleton(new JwtService(secretKey, issuer, audience));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
        };
    });

// Define the CanViewOrders policy
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CanViewOrders", policy =>
        policy.RequireRole("Admin"));
});

// Register Dapper Repositories
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();

builder.Services.AddControllers();
var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
