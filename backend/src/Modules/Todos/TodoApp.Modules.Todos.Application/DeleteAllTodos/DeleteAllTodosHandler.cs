using TodoApp.Modules.Todos.Application.Abstractions;
using TodoApp.Modules.Todos.Application.IntegrationEvents;
using TodoApp.Shared.Kernel.Events;

namespace TodoApp.Modules.Todos.Application.DeleteAllTodos;

public sealed class DeleteAllTodosHandler( // primary-конструктор с репозиторием задач, unit of work, источником времени и публикатором событий
    ITodoRepository repository,
    ITodosUnitOfWork unitOfWork,
    TimeProvider timeProvider,
    IEventPublisher eventPublisher)
{
    /* --- Methods --- */
    /// <summary>Deletes every todo item. Succeeds when the list is already empty.</summary>
    public async Task HandleAsync(CancellationToken cancellationToken) // удаляет все задачи, сохраняет изменения и публикует ListItemChanged для каждой
    {
        var items = await repository.GetAllAsync(cancellationToken);
        foreach (var item in items)
        {
            repository.Remove(item);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var removedAt = timeProvider.GetUtcNow();
        foreach (var item in items)
        {
            await eventPublisher.PublishAsync(TodoListEvents.Removed(item.Id, removedAt), cancellationToken);
        }
    }
}
