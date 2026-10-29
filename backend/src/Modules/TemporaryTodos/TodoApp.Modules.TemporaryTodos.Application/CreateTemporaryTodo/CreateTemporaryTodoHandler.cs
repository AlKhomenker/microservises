using TodoApp.Modules.TemporaryTodos.Application.Abstractions;
using TodoApp.Modules.TemporaryTodos.Application.Contracts;
using TodoApp.Modules.TemporaryTodos.Application.IntegrationEvents;
using TodoApp.Modules.TemporaryTodos.Domain;
using TodoApp.Shared.Kernel;
using TodoApp.Shared.Kernel.Events;

namespace TodoApp.Modules.TemporaryTodos.Application.CreateTemporaryTodo;

public sealed class CreateTemporaryTodoHandler( // primary-конструктор с репозиторием временных задач, unit of work, источником времени и публикатором событий
    ITemporaryTodoRepository repository,
    ITemporaryTodosUnitOfWork unitOfWork,
    TimeProvider timeProvider,
    IEventPublisher eventPublisher)
{
    /* --- Methods --- */
    public async Task<OperationResult<TemporaryTodoItemDto>> HandleAsync(CreateTemporaryTodoCommand command, CancellationToken cancellationToken) // создаёт и сохраняет новую временную задачу, после сохранения публикует ListItemChanged; возвращает OperationResult<TemporaryTodoItemDto> с задачей или ошибкой валидации
    {
        ArgumentNullException.ThrowIfNull(command);

        var now = timeProvider.GetUtcNow();
        var created = TemporaryTodoItem.Create(command.Text, command.StartDate, now);
        if (!created.IsSuccess)
        {
            return created.Error;
        }

        repository.Add(created.MyValue);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await eventPublisher.PublishAsync(TemporaryTodoListEvents.Added(created.MyValue.Id, now), cancellationToken);

        return TemporaryTodoItemDto.FromDomain(created.MyValue, timeProvider.GetUtcToday());
    }
}
