namespace TodoApp.Modules.TemporaryTodos.Infrastructure.Persistence;

internal sealed class TemporaryTodosDatabaseOptions
{
    /* --- Properties --- */
    public const string SectionName = "Modules:TemporaryTodos:Database"; // имя секции конфигурации
    /// <summary>Applies pending EF Core migrations when the host starts.</summary>
    public bool ApplyMigrationsOnStartup { get; init; } // применять миграции при старте; init - задаётся только из конфигурации
}
