using TodoApp.Modules.Todos.Application.Abstractions;
using TodoApp.Modules.Todos.Domain;

namespace TodoApp.Modules.Todos.UnitTests.Fakes;

/// <summary>Test double that keeps items in memory and counts commits.</summary>
internal sealed class InMemoryTodoStore : ITodoRepository, ITodosUnitOfWork
{
    private readonly List<TodoItem> _pending = [];
    private readonly List<TodoItem> _removed = [];

    public List<TodoItem> Items { get; } = [];

    public int SaveChangesCalls { get; private set; }

    public Task<IReadOnlyList<TodoItem>> GetAllAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<TodoItem>>(Items.OrderByDescending(i => i.CreateDate).ToList());

    public Task<TodoItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        Task.FromResult(Items.SingleOrDefault(i => i.Id == id));

    public void Add(TodoItem item) => _pending.Add(item);

    public void Remove(TodoItem item) => _removed.Add(item);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var changes = _pending.Count + _removed.Count;

        Items.AddRange(_pending);
        Items.RemoveAll(_removed.Contains);
        _pending.Clear();
        _removed.Clear();
        SaveChangesCalls++;

        return Task.FromResult(changes);
    }
}
