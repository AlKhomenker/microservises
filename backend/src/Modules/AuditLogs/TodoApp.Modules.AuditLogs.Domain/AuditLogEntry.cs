namespace TodoApp.Modules.AuditLogs.Domain;

public sealed class AuditLogEntry
{
    /* --- Properties --- */
    public const int ListNameMaxLength = 100; // максимальная длина имени списка
    public Guid Id { get; private set; } // id записи; private set - нельзя изменить снаружи
    public AuditLogType Type { get; private set; } // что произошло: добавление или удаление; private set - нельзя изменить снаружи
    public DateOnly Date { get; private set; } // день действия (UTC); private set - нельзя изменить снаружи
    public string ListName { get; private set; } = string.Empty; // в каком списке было действие; private set - нельзя изменить снаружи
    public Guid ItemId { get; private set; } // id элемента списка; private set - нельзя изменить снаружи

    /* --- Constructors --- */
    private AuditLogEntry() { } // пустой конструктор, нужен для EF Core
    private AuditLogEntry(Guid id, AuditLogType type, DateOnly date, string listName, Guid itemId) // конструктор со всеми полями записи
    {
        Id = id;
        Type = type;
        Date = date;
        ListName = listName;
        ItemId = itemId;
    }

    /* --- Methods --- */
    public static AuditLogEntry Create(AuditLogType type, string listName, Guid itemId, DateTimeOffset occurredAt) // фабричный метод Create, возвращает запись журнала с датой действия по UTC
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(listName);

        return new AuditLogEntry(
            Guid.CreateVersion7(occurredAt),
            type,
            DateOnly.FromDateTime(occurredAt.UtcDateTime),
            listName,
            itemId);
    }
}
