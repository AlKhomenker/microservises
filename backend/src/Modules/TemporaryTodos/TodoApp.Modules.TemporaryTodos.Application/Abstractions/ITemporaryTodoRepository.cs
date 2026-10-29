using TodoApp.Modules.TemporaryTodos.Domain;

namespace TodoApp.Modules.TemporaryTodos.Application.Abstractions;

public interface ITemporaryTodoRepository
{
    /* --- Methods --- */
    /// <summary>Returns all items ordered by <see cref="TemporaryTodoItem.StartDate"/>, latest first.</summary>
    Task<IReadOnlyList<TemporaryTodoItem>> GetAllAsync(CancellationToken cancellationToken); // возвращает все временные задачи, сначала с поздней датой начала
    Task<TemporaryTodoItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken); // возвращает временную задачу с этим id или null, если её нет
    void Add(TemporaryTodoItem item); // добавляет временную задачу; сохраняется при SaveChangesAsync
    void Remove(TemporaryTodoItem item); // удаляет временную задачу; сохраняется при SaveChangesAsync
}
