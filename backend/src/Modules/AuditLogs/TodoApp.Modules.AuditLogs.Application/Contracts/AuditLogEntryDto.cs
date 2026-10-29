using System.Text.Json;
using System.Text.Json.Serialization;
using TodoApp.Modules.AuditLogs.Domain;

namespace TodoApp.Modules.AuditLogs.Application.Contracts;

public sealed record AuditLogEntryDto( // primary-конструктор с id, типом, датой, именем списка и id элемента
    Guid Id,
    [property: JsonConverter(typeof(AuditLogTypeJsonConverter))] AuditLogType Type,
    DateOnly Date,
    string ListName,
    Guid ItemId)
{
    /* --- Methods --- */
    public static AuditLogEntryDto FromDomain(AuditLogEntry entry) // фабричный метод FromDomain, превращает AuditLogEntry в AuditLogEntryDto
    {
        ArgumentNullException.ThrowIfNull(entry);
        return new AuditLogEntryDto(entry.Id, entry.Type, entry.Date, entry.ListName, entry.ItemId);
    }
}

/// <summary>Writes <see cref="AuditLogType"/> as "add" / "removed".</summary>
public sealed class AuditLogTypeJsonConverter() // конвертер типа записи в строку camelCase
    : JsonStringEnumConverter<AuditLogType>(JsonNamingPolicy.CamelCase);
