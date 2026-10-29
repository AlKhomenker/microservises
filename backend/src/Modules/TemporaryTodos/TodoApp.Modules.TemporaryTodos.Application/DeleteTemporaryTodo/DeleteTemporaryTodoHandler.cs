using TodoApp.Modules.TemporaryTodos.Application.Abstractions;
using TodoApp.Modules.TemporaryTodos.Application.IntegrationEvents;
using TodoApp.Modules.TemporaryTodos.Domain;
using TodoApp.Shared.Kernel;
using TodoApp.Shared.Kernel.Events;

namespace TodoApp.Modules.TemporaryTodos.Application.DeleteTemporaryTodo;

public sealed class DeleteTemporaryTodoHandler( // primary-конструктор с репозиторием временных задач, unit of work, источником времени и публикатором событий
    ITemporaryTodoRepository repository,
    ITemporaryTodosUnitOfWork unitOfWork,
    TimeProvider timeProvider,
    IEventPublisher eventPublisher)
{
    /* --- Methods --- */
    public async Task<OperationResult> HandleAsync(Guid id, CancellationToken cancellationToken) // удаляет временную задачу с этим id и публикует ListItemChanged; если её нет, возвращает неуспешный OperationResult
    {
        var item = await repository.GetByIdAsync(id, cancellationToken);
        if (item is null)
        {
            return OperationResultFactory.Failure(TemporaryTodoErrors.NotFound(id));
        }

        repository.Remove(item);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await eventPublisher.PublishAsync(TemporaryTodoListEvents.Removed(item.Id, timeProvider.GetUtcNow()), cancellationToken);

        return OperationResultFactory.Success();
    }
}
