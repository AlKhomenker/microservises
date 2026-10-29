using Microsoft.EntityFrameworkCore;
using TodoApp.Modules.TemporaryTodos.Application.Abstractions;
using TodoApp.Modules.TemporaryTodos.Domain;

namespace TodoApp.Modules.TemporaryTodos.Infrastructure.Persistence;

internal sealed class TemporaryTodoRepository(TemporaryTodosDbContext dbContext) : ITemporaryTodoRepository // primary-конструктор с DbContext модуля TemporaryTodos
{
    /* --- Methods --- */
    public async Task<IReadOnlyList<TemporaryTodoItem>> GetAllAsync(CancellationToken cancellationToken) => // возвращает все временные задачи, сначала с поздней датой начала, без отслеживания изменений
        await dbContext.TemporaryTodoItems
            .AsNoTracking()
            .OrderByDescending(x => x.StartDate)
            .ThenByDescending(x => x.Id)
            .ToListAsync(cancellationToken);
    public Task<TemporaryTodoItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => // возвращает временную задачу с этим id или null, если её нет
        dbContext.TemporaryTodoItems.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    public void Add(TemporaryTodoItem item) => dbContext.TemporaryTodoItems.Add(item); // добавляет временную задачу в DbContext
    public void Remove(TemporaryTodoItem item) => dbContext.TemporaryTodoItems.Remove(item); // удаляет временную задачу из DbContext
}
