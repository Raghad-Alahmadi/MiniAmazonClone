# MiniAmazon Clone

A lightweight e-commerce platform built with ASP.NET Core that simulates core Amazon functionality.

## Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or newer
- [Visual Studio 2022](https://visualstudio.microsoft.com/vs/) or [Visual Studio Code](https://code.visualstudio.com/)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (or SQL Server Express)

## Project Setup

### Clone the Repository

```bash
git clone https://github.com/Raghad-Alahmadi/MiniAmazonClone.git
cd MiniAmazonClone
```

### Database Setup

1. Update the connection string in `appsettings.json` to point to your SQL Server instance:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=MiniAmazonClone;Trusted_Connection=True;MultipleActiveResultSets=true"
}
```

2. Run database migrations to create the database schema:

```bash
dotnet ef database update
```

## Running the Application

### Using Visual Studio
1. Open the solution file `MiniAmazonClone.sln` in Visual Studio
2. Set the startup project to `MiniAmazonClone`
3. Press F5 to run the application

### Using Command Line
```bash
dotnet run --project MiniAmazonClone
```

The application should now be running at `https://localhost:5001` and `http://localhost:5000`

## Testing

### Running Tests
```bash
dotnet test
```

### API Testing
The project includes a `.http` file that can be used with the REST Client extension in VS Code:

1. Open `MiniAmazonClone.http`
2. Use the VS Code REST Client extension to send requests directly from the editor

## Project Structure

- **Controllers/**: API endpoints
- **Models/**: Domain models and DTOs
- **Data/**: Database context and configuration
- **Services/**: Business logic implementations
- **Repositories/**: Data access layer
- **Migrations/**: EF Core database migrations

## Performance Testing

The project includes performance comparison tools:

```bash
cd PerformanceComparison
dotnet run
```

Results are stored in `performance-results.txt`

## Contributing

1. Fork the repository
2. Create a feature branch: `git checkout -b feature-name`
3. Commit your changes: `git commit -m 'Add some feature'`
4. Push to the branch: `git push origin feature-name`
5. Submit a pull request
```
