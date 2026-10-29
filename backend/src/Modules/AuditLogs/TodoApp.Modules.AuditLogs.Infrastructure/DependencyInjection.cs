using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TodoApp.Modules.AuditLogs.Application.Abstractions;
using TodoApp.Modules.AuditLogs.Infrastructure.Persistence;

namespace TodoApp.Modules.AuditLogs.Infrastructure;

public static class DependencyInjection
{
    /* --- Properties --- */
    public const string ConnectionStringName = "AuditLogs"; // имя строки подключения в конфигурации

    /* --- Methods --- */
    public static IServiceCollection AddAuditLogsInfrastructure( // регистрирует в DI DbContext, репозиторий, мигратор и health check; возвращает services для цепочки вызовов
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(ConnectionStringName)
            ?? throw new InvalidOperationException($"Connection string '{ConnectionStringName}' is not configured.");

        services.AddDbContext<AuditLogsDbContext>(options =>
            options.UseSqlite(connectionString, sqlite =>
                sqlite.MigrationsHistoryTable(AuditLogsDbContext.MigrationsHistoryTable)));

        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<IAuditLogsUnitOfWork>(sp => sp.GetRequiredService<AuditLogsDbContext>());

        services.AddOptions<AuditLogsDatabaseOptions>()
            .Bind(configuration.GetSection(AuditLogsDatabaseOptions.SectionName));
        services.AddHostedService<AuditLogsDatabaseMigrator>();

        services.AddHealthChecks()
            .AddDbContextCheck<AuditLogsDbContext>("audit-logs-db", tags: ["ready"]);

        return services;
    }
}
