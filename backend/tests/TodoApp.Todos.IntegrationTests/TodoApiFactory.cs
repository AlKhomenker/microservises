using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Time.Testing;
using TodoApp.Modules.Todos.Infrastructure.Persistence;

namespace TodoApp.Todos.IntegrationTests;

/// <summary>
/// Runs the real Todos host against a private in-memory SQLite database.
/// The open connection keeps the shared in-memory database alive for the factory's lifetime.
/// </summary>
public sealed class TodoApiFactory : WebApplicationFactory<Program>
{
    // Module name = name of its connection string and of its "Modules:<Name>" config section.
    private const string Module = "Todos";

    private readonly string _connectionString = $"Data Source=file:{Module}-tests-{Guid.NewGuid():N}?mode=memory&cache=shared";

    private readonly SqliteConnection _keepAliveConnection;

    public TodoApiFactory()
    {
        _keepAliveConnection = new SqliteConnection(_connectionString);
        _keepAliveConnection.Open();
    }

    public FakeTimeProvider Clock { get; } = new(new DateTimeOffset(2026, 10, 21, 8, 0, 0, TimeSpan.Zero));

    public async Task ResetDatabaseAsync()
    {
        await using var scope = Services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<TodosDbContext>().TodoItems.ExecuteDeleteAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting($"ConnectionStrings:{Module}", _connectionString);
        builder.UseSetting($"Modules:{Module}:Database:ApplyMigrationsOnStartup", "true");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<TimeProvider>();
            services.AddSingleton<TimeProvider>(Clock);
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _keepAliveConnection.Dispose();
        }
    }
}
