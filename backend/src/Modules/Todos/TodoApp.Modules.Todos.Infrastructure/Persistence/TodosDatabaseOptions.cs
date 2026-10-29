namespace TodoApp.Modules.Todos.Infrastructure.Persistence;

internal sealed class TodosDatabaseOptions
{
    /* --- Properties --- */
    public const string SectionName = "Modules:Todos:Database"; // имя секции конфигурации
    /// <summary>Applies pending EF Core migrations when the host starts.</summary>
    public bool ApplyMigrationsOnStartup { get; init; } // применять миграции при старте; init - задаётся только из конфигурации
}
