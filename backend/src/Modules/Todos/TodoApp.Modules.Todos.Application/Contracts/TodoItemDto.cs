using TodoApp.Modules.Todos.Domain;

namespace TodoApp.Modules.Todos.Application.Contracts;

public sealed record TodoItemDto(Guid Id, string Text, DateTimeOffset CreateDate) // primary-конструктор с id, текстом и датой создания задачи
{
    /* --- Methods --- */
    public static TodoItemDto FromDomain(TodoItem item) // фабричный метод FromDomain, превращает TodoItem в TodoItemDto
    {
        ArgumentNullException.ThrowIfNull(item);
        return new TodoItemDto(item.Id, item.Text, item.CreateDate);
    }
}
