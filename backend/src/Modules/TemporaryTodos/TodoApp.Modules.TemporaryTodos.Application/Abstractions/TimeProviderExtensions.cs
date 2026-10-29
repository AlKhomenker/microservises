namespace TodoApp.Modules.TemporaryTodos.Application.Abstractions;

internal static class TimeProviderExtensions
{
    /* --- Methods --- */
    public static DateOnly GetUtcToday(this TimeProvider timeProvider) => // возвращает текущую дату по UTC
        DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);
}
