using TodoApp.Modules.TemporaryTodos.Application.Abstractions;
using TodoApp.Modules.TemporaryTodos.Application.IntegrationEvents;
using TodoApp.Shared.Kernel.Events;

namespace TodoApp.Modules.TemporaryTodos.Application.DeleteAllTemporaryTodos;

public sealed class DeleteAllTemporaryTodosHandler( // primary-конструктор с репозиторием временных задач, unit of work, источником времени и публикатором событий
    ITemporaryTodoRepository repository,
    ITemporaryTodosUnitOfWork unitOfWork,
    TimeProvider timeProvider,
    IEventPublisher eventPublisher)
{
    /* --- Methods --- */
    /// <summary>Deletes every temporary todo item. Succeeds when the list is already empty.</summary>
    public async Task HandleAsync(CancellationToken cancellationToken) // удаляет все временные задачи, сохраняет изменения и публикует ListItemChanged для каждой
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
            await eventPublisher.PublishAsync(TemporaryTodoListEvents.Removed(item.Id, removedAt), cancellationToken);
        }
    }
}
