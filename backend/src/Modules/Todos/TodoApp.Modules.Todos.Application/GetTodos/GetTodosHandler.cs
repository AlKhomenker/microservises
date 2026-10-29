using TodoApp.Modules.Todos.Application.Abstractions;
using TodoApp.Modules.Todos.Application.Contracts;

namespace TodoApp.Modules.Todos.Application.GetTodos;

public sealed class GetTodosHandler(ITodoRepository repository) // primary-конструктор с репозиторием задач
{
    /* --- Methods --- */
    /// <summary>Returns all todo items, newest first.</summary>
    public async Task<IReadOnlyList<TodoItemDto>> HandleAsync(CancellationToken cancellationToken) // читает все задачи и возвращает их списком TodoItemDto
    {
        var items = await repository.GetAllAsync(cancellationToken);

        return items.Select(TodoItemDto.FromDomain).ToList();
    }
}
