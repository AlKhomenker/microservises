using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace TodoApp.Modules.TemporaryTodos.Infrastructure.Persistence;

internal sealed partial class TemporaryTodosDatabaseMigrator( // primary-конструктор с фабрикой scope, настройками БД и логгером
    IServiceScopeFactory scopeFactory,
    IOptions<TemporaryTodosDatabaseOptions> options,
    ILogger<TemporaryTodosDatabaseMigrator> logger) : IHostedService
{
    /* --- Methods --- */
    public async Task StartAsync(CancellationToken cancellationToken) // применяет новые миграции при старте хоста, если ApplyMigrationsOnStartup = true
    {
        if (!options.Value.ApplyMigrationsOnStartup)
        {
            return;
        }

        await using var scope = scopeFactory.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TemporaryTodosDbContext>();

        await dbContext.Database.MigrateAsync(cancellationToken);
        LogMigrated(logger);
    }
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask; // при остановке хоста ничего не делает
    [LoggerMessage(Level = LogLevel.Information, Message = "TemporaryTodos database migrations applied.")]
    private static partial void LogMigrated(ILogger logger); // пишет в лог, что миграции применены; тело метода генерируется
}
