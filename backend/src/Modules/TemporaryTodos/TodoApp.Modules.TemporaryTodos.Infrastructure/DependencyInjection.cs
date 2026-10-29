using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TodoApp.Modules.TemporaryTodos.Application.Abstractions;
using TodoApp.Modules.TemporaryTodos.Infrastructure.Persistence;

namespace TodoApp.Modules.TemporaryTodos.Infrastructure;

public static class DependencyInjection
{
    /* --- Properties --- */
    public const string ConnectionStringName = "TemporaryTodos"; // имя строки подключения в конфигурации

    /* --- Methods --- */
    public static IServiceCollection AddTemporaryTodosInfrastructure( // регистрирует в DI DbContext, репозиторий, мигратор и health check; возвращает services для цепочки вызовов
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(ConnectionStringName)
            ?? throw new InvalidOperationException($"Connection string '{ConnectionStringName}' is not configured.");

        services.AddDbContext<TemporaryTodosDbContext>(options =>
            options.UseSqlite(connectionString, sqlite =>
                sqlite.MigrationsHistoryTable(TemporaryTodosDbContext.MigrationsHistoryTable)));

        services.AddScoped<ITemporaryTodoRepository, TemporaryTodoRepository>();
        services.AddScoped<ITemporaryTodosUnitOfWork>(sp => sp.GetRequiredService<TemporaryTodosDbContext>());

        services.AddOptions<TemporaryTodosDatabaseOptions>()
            .Bind(configuration.GetSection(TemporaryTodosDatabaseOptions.SectionName));
        services.AddHostedService<TemporaryTodosDatabaseMigrator>();

        services.AddHealthChecks()
            .AddDbContextCheck<TemporaryTodosDbContext>("temporary-todos-db", tags: ["ready"]);

        return services;
    }
}
