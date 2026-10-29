using System.Text.Json;
using System.Text.Json.Serialization;
using TodoApp.Modules.TemporaryTodos.Domain;

namespace TodoApp.Modules.TemporaryTodos.Application.Contracts;

public sealed record TemporaryTodoItemDto( // primary-конструктор с id, текстом, датой начала и статусом временной задачи
    Guid Id,
    string Text,
    DateOnly StartDate,
    [property: JsonConverter(typeof(TemporaryTodoStatusJsonConverter))] TemporaryTodoStatus Status)
{
    /* --- Methods --- */
    public static TemporaryTodoItemDto FromDomain(TemporaryTodoItem item, DateOnly today) // фабричный метод FromDomain, превращает TemporaryTodoItem в TemporaryTodoItemDto со статусом на дату today
    {
        ArgumentNullException.ThrowIfNull(item);
        return new TemporaryTodoItemDto(item.Id, item.Text, item.StartDate, item.GetStatus(today));
    }
}

/// <summary>Writes <see cref="TemporaryTodoStatus"/> as "active" / "expired".</summary>
public sealed class TemporaryTodoStatusJsonConverter() // конвертер статуса в строку camelCase
    : JsonStringEnumConverter<TemporaryTodoStatus>(JsonNamingPolicy.CamelCase);
