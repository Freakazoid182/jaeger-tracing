namespace telemetry.worker.Data;

using Dapper;
using Microsoft.Data.SqlClient;
using telemetry.worker.Models;

public interface IProductsRepository
{
    Task InsertAsync(Product product);
}

public class ProductsRepository : IProductsRepository
{
    private readonly IConfiguration _configuration;

    public ProductsRepository(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task InsertAsync(Product product)
    {
        using (var connection = new SqlConnection(_configuration.GetValue<string>("Sql:ConnectionString")))

        await connection.ExecuteAsync("INSERT INTO [Products]([Id], [Name], [CreatedDate]) VALUES (@Id, @Name, @CreatedDate)", product);
    }
}