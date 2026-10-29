using TodoApp.Modules.TemporaryTodos.Application.Abstractions;
using TodoApp.Modules.TemporaryTodos.Domain;

namespace TodoApp.Modules.TemporaryTodos.UnitTests.Fakes;

/// <summary>Test double that keeps items in memory and counts commits.</summary>
internal sealed class InMemoryTemporaryTodoStore : ITemporaryTodoRepository, ITemporaryTodosUnitOfWork
{
    private readonly List<TemporaryTodoItem> _pending = [];
    private readonly List<TemporaryTodoItem> _removed = [];

    public List<TemporaryTodoItem> Items { get; } = [];

    public int SaveChangesCalls { get; private set; }

    public Task<IReadOnlyList<TemporaryTodoItem>> GetAllAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<TemporaryTodoItem>>(
            Items.OrderByDescending(i => i.StartDate).ThenByDescending(i => i.Id).ToList());

    public Task<TemporaryTodoItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        Task.FromResult(Items.SingleOrDefault(i => i.Id == id));

    public void Add(TemporaryTodoItem item) => _pending.Add(item);

    public void Remove(TemporaryTodoItem item) => _removed.Add(item);

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
