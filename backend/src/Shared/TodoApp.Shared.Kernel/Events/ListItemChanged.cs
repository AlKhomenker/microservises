namespace TodoApp.Shared.Kernel.Events;

public enum ListItemChangeType
{
    Added, // элемент добавлен в список
    Removed, // элемент удалён из списка
}

/// <summary>Published by a list module after an item was added to or removed from its list and the change was saved.</summary>
public sealed record ListItemChanged( // primary-конструктор с именем списка, id элемента, типом изменения и временем изменения
    string ListName,
    Guid ItemId,
    ListItemChangeType ChangeType,
    DateTimeOffset OccurredAt) : IIntegrationEvent;
