using TodoApp.Modules.Todos.Application.Abstractions;
using TodoApp.Modules.Todos.Application.IntegrationEvents;
using TodoApp.Modules.Todos.Domain;
using TodoApp.Shared.Kernel;
using TodoApp.Shared.Kernel.Events;

namespace TodoApp.Modules.Todos.Application.DeleteTodo;

public sealed class DeleteTodoHandler( // primary-конструктор с репозиторием задач, unit of work, источником времени и публикатором событий
    ITodoRepository repository,
    ITodosUnitOfWork unitOfWork,
    TimeProvider timeProvider,
    IEventPublisher eventPublisher)
{
    /* --- Methods --- */
    public async Task<OperationResult> HandleAsync(Guid id, CancellationToken cancellationToken) // удаляет задачу с этим id и публикует ListItemChanged; если её нет, возвращает неуспешный OperationResult
    {
        var item = await repository.GetByIdAsync(id, cancellationToken);
        if (item is null)
        {
            return OperationResultFactory.Failure(TodoErrors.NotFound(id));
        }

        repository.Remove(item);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await eventPublisher.PublishAsync(TodoListEvents.Removed(item.Id, timeProvider.GetUtcNow()), cancellationToken);

        return OperationResultFactory.Success();
    }
}
