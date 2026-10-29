using TodoApp.Modules.TemporaryTodos.Application.Abstractions;
using TodoApp.Modules.TemporaryTodos.Application.Contracts;

namespace TodoApp.Modules.TemporaryTodos.Application.GetTemporaryTodos;

public sealed class GetTemporaryTodosHandler(ITemporaryTodoRepository repository, TimeProvider timeProvider) // primary-конструктор с репозиторием временных задач и источником времени
{
    /* --- Methods --- */
    /// <summary>Returns all temporary todo items with their status for today, latest start date first.</summary>
    public async Task<IReadOnlyList<TemporaryTodoItemDto>> HandleAsync(CancellationToken cancellationToken) // читает все временные задачи и возвращает их списком TemporaryTodoItemDto
    {
        var items = await repository.GetAllAsync(cancellationToken);
        var today = timeProvider.GetUtcToday();

        return items.Select(item => TemporaryTodoItemDto.FromDomain(item, today)).ToList();
    }
}
