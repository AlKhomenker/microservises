using TodoApp.Modules.Todos.Domain;

namespace TodoApp.Modules.Todos.Application.Abstractions;

public interface ITodoRepository
{
    /* --- Methods --- */
    /// <summary>Returns all items ordered by <see cref="TodoItem.CreateDate"/>, newest first.</summary>
    Task<IReadOnlyList<TodoItem>> GetAllAsync(CancellationToken cancellationToken); // возвращает все задачи, сначала новые
    Task<TodoItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken); // возвращает задачу с этим id или null, если её нет
    void Add(TodoItem item); // добавляет задачу; сохраняется при SaveChangesAsync
    void Remove(TodoItem item); // удаляет задачу; сохраняется при SaveChangesAsync
}
