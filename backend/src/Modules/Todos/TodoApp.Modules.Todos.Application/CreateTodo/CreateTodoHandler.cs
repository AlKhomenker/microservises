using TodoApp.Modules.Todos.Application.Abstractions;
using TodoApp.Modules.Todos.Application.Contracts;
using TodoApp.Modules.Todos.Application.IntegrationEvents;
using TodoApp.Modules.Todos.Domain;
using TodoApp.Shared.Kernel;
using TodoApp.Shared.Kernel.Events;

namespace TodoApp.Modules.Todos.Application.CreateTodo;

public sealed class CreateTodoHandler( // primary-конструктор с репозиторием задач, unit of work, источником времени и публикатором событий
    ITodoRepository repository,
    ITodosUnitOfWork unitOfWork,
    TimeProvider timeProvider,
    IEventPublisher eventPublisher)
{
    /* --- Methods --- */
    public async Task<OperationResult<TodoItemDto>> HandleAsync(CreateTodoCommand command, CancellationToken cancellationToken) // создаёт и сохраняет новую задачу, после сохранения публикует ListItemChanged; возвращает OperationResult<TodoItemDto> с задачей или ошибкой валидации
    {
        ArgumentNullException.ThrowIfNull(command);

        var created = TodoItem.Create(command.Text, timeProvider.GetUtcNow());
        if (!created.IsSuccess)
        {
            return created.Error;
        }

        repository.Add(created.MyValue);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await eventPublisher.PublishAsync(TodoListEvents.Added(created.MyValue.Id, created.MyValue.CreateDate), cancellationToken);

        return TodoItemDto.FromDomain(created.MyValue);
    }
}
