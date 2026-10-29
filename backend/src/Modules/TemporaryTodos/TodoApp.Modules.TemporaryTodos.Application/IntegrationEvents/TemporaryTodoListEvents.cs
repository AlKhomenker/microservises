using TodoApp.Shared.Kernel.Events;

namespace TodoApp.Modules.TemporaryTodos.Application.IntegrationEvents;

/// <summary>Builds the integration events this module publishes about its list.</summary>
internal static class TemporaryTodoListEvents
{
    /* --- Properties --- */
    public const string ListName = "temporary-todos"; // имя списка в событиях

    /* --- Methods --- */
    public static ListItemChanged Added(Guid itemId, DateTimeOffset occurredAt) => // событие «временная задача добавлена»
        new(ListName, itemId, ListItemChangeType.Added, occurredAt);
    public static ListItemChanged Removed(Guid itemId, DateTimeOffset occurredAt) => // событие «временная задача удалена»
        new(ListName, itemId, ListItemChangeType.Removed, occurredAt);
}
