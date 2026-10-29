namespace TodoApp.Modules.AuditLogs.Infrastructure.Persistence;

internal sealed class AuditLogsDatabaseOptions
{
    /* --- Properties --- */
    public const string SectionName = "Modules:AuditLogs:Database"; // имя секции конфигурации
    /// <summary>Applies pending EF Core migrations when the host starts.</summary>
    public bool ApplyMigrationsOnStartup { get; init; } // применять миграции при старте; init - задаётся только из конфигурации
}
