using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TodoApp.Modules.Todos.Application.Abstractions;
using TodoApp.Modules.Todos.Infrastructure.Persistence;

namespace TodoApp.Modules.Todos.Infrastructure;

public static class DependencyInjection
{
    /* --- Properties --- */
    public const string ConnectionStringName = "Todos"; // имя строки подключения в конфигурации

    /* --- Methods --- */
    public static IServiceCollection AddTodosInfrastructure( // регистрирует в DI DbContext, репозиторий, мигратор и health check; возвращает services для цепочки вызовов
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(ConnectionStringName)
            ?? throw new InvalidOperationException($"Connection string '{ConnectionStringName}' is not configured.");

        services.AddDbContext<TodosDbContext>(options =>
            options.UseSqlite(connectionString, sqlite =>
                sqlite.MigrationsHistoryTable(TodosDbContext.MigrationsHistoryTable)));

        services.AddScoped<ITodoRepository, TodoRepository>();
        services.AddScoped<ITodosUnitOfWork>(sp => sp.GetRequiredService<TodosDbContext>());

        services.AddOptions<TodosDatabaseOptions>()
            .Bind(configuration.GetSection(TodosDatabaseOptions.SectionName));
        services.AddHostedService<TodosDatabaseMigrator>();

        services.AddHealthChecks()
            .AddDbContextCheck<TodosDbContext>("todos-db", tags: ["ready"]);

        return services;
    }
}
