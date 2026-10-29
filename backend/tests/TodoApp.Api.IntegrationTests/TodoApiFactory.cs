using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Time.Testing;
using TodoApp.Modules.AuditLogs.Infrastructure.Persistence;
using TodoApp.Modules.TemporaryTodos.Infrastructure.Persistence;
using TodoApp.Modules.Todos.Infrastructure.Persistence;

namespace TodoApp.Api.IntegrationTests;

/// <summary>
/// Runs the real host against private in-memory SQLite databases, one per module.
/// The open connections keep the shared in-memory databases alive for the factory's lifetime.
/// </summary>
public sealed class TodoApiFactory : WebApplicationFactory<Program>
{
    // Module name = name of its connection string and of its "Modules:<Name>" config section.
    private static readonly string[] Modules = ["Todos", "TemporaryTodos", "AuditLogs"];

    private readonly Dictionary<string, string> _connectionStrings = Modules.ToDictionary(
        module => module,
        module => $"Data Source=file:{module}-tests-{Guid.NewGuid():N}?mode=memory&cache=shared");

    private readonly List<SqliteConnection> _keepAliveConnections = [];

    public TodoApiFactory()
    {
        foreach (var connectionString in _connectionStrings.Values)
        {
            var connection = new SqliteConnection(connectionString);
            connection.Open();
            _keepAliveConnections.Add(connection);
        }
    }

    public FakeTimeProvider Clock { get; } = new(new DateTimeOffset(2026, 10, 21, 8, 0, 0, TimeSpan.Zero));

    public async Task ResetDatabaseAsync()
    {
        await using var scope = Services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<TodosDbContext>().TodoItems.ExecuteDeleteAsync();
        await scope.ServiceProvider.GetRequiredService<TemporaryTodosDbContext>().TemporaryTodoItems.ExecuteDeleteAsync();
        await scope.ServiceProvider.GetRequiredService<AuditLogsDbContext>().AuditLogEntries.ExecuteDeleteAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        foreach (var (module, connectionString) in _connectionStrings)
        {
            builder.UseSetting($"ConnectionStrings:{module}", connectionString);
            builder.UseSetting($"Modules:{module}:Database:ApplyMigrationsOnStartup", "true");
        }

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
            foreach (var connection in _keepAliveConnections)
            {
                connection.Dispose();
            }
        }
    }
}
