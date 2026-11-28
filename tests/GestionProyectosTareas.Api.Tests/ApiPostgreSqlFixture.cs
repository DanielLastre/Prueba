using GestionProyectosTareas.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using Xunit;

namespace GestionProyectosTareas.Api.Tests;

public class ApiPostgreSqlFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:16-alpine")
        .Build();

    private WebApplicationFactory<Program>? _factory;

    public HttpClient Client => _factory!.CreateClient();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        var connectionString = _container.GetConnectionString();

        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((_, config) =>
                {
                    config.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["ConnectionStrings:DefaultConnection"] = connectionString
                    });
                });
            });

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await dbContext.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        if (_factory is not null)
            await _factory.DisposeAsync();

        try
        {
            await _container.DisposeAsync();
        }
        catch
        {
            // El daemon de Docker (snap) puede fallar al eliminar el contenedor;
            // no debe marcar los tests como fallidos.
        }
    }
}
