using Microsoft.EntityFrameworkCore;
using TodoApp.Modules.AuditLogs.Application.Abstractions;
using TodoApp.Modules.AuditLogs.Domain;

namespace TodoApp.Modules.AuditLogs.Infrastructure.Persistence;

internal sealed class AuditLogsDbContext(DbContextOptions<AuditLogsDbContext> options) // primary-конструктор с настройками DbContext
    : DbContext(options), IAuditLogsUnitOfWork
{
    /* --- Properties --- */
    internal const string MigrationsHistoryTable = "__EFMigrationsHistory_AuditLogs"; // таблица истории миграций этого модуля
    public DbSet<AuditLogEntry> AuditLogEntries => Set<AuditLogEntry>(); // таблица записей журнала

    /* --- Methods --- */
    protected override void OnModelCreating(ModelBuilder modelBuilder) // применяет все конфигурации сущностей из этой сборки
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AuditLogsDbContext).Assembly);
    }
}
