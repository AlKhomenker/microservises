using Microsoft.EntityFrameworkCore;
using TodoApp.Modules.Todos.Application.Abstractions;
using TodoApp.Modules.Todos.Domain;

namespace TodoApp.Modules.Todos.Infrastructure.Persistence;

internal sealed class TodoRepository(TodosDbContext dbContext) : ITodoRepository // primary-конструктор с DbContext модуля Todos
{
    /* --- Methods --- */
    public async Task<IReadOnlyList<TodoItem>> GetAllAsync(CancellationToken cancellationToken) => // возвращает все задачи, сначала новые, без отслеживания изменений
        await dbContext.TodoItems
            .AsNoTracking()
            .OrderByDescending(x => x.CreateDate)
            .ToListAsync(cancellationToken);
    public Task<TodoItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => // возвращает задачу с этим id или null, если её нет
        dbContext.TodoItems.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    public void Add(TodoItem item) => dbContext.TodoItems.Add(item); // добавляет задачу в DbContext
    public void Remove(TodoItem item) => dbContext.TodoItems.Remove(item); // удаляет задачу из DbContext
}
