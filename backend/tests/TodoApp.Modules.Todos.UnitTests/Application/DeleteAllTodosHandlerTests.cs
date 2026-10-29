using Microsoft.Extensions.Time.Testing;
using TodoApp.Modules.Todos.Application.DeleteAllTodos;
using TodoApp.Modules.Todos.Domain;
using TodoApp.Modules.Todos.UnitTests.Fakes;
using TodoApp.Shared.Kernel.Events;

namespace TodoApp.Modules.Todos.UnitTests.Application;

public sealed class DeleteAllTodosHandlerTests
{
    private readonly InMemoryTodoStore _store = new();
    private readonly FakeTimeProvider _clock = new(new DateTimeOffset(2026, 10, 21, 9, 30, 0, TimeSpan.Zero));
    private readonly FakeEventPublisher _events = new();
    private readonly DeleteAllTodosHandler _handler;

    public DeleteAllTodosHandlerTests() => _handler = new DeleteAllTodosHandler(_store, _store, _clock, _events);

    [Fact]
    public async Task HandleAsync_WithItems_RemovesAllInOneCommit()
    {
        var first = TodoItem.Create("first", DateTimeOffset.UtcNow).MyValue;
        var second = TodoItem.Create("second", DateTimeOffset.UtcNow).MyValue;
        _store.Items.AddRange([first, second]);

        await _handler.HandleAsync(TestContext.Current.CancellationToken);

        Assert.Empty(_store.Items);
        Assert.Equal(1, _store.SaveChangesCalls);
        var removed = _events.Published.Cast<ListItemChanged>().ToList();
        Assert.All(removed, e => Assert.Equal(ListItemChangeType.Removed, e.ChangeType));
        Assert.Equal(new[] { first.Id, second.Id }.Order(), removed.Select(e => e.ItemId).Order());
    }

    [Fact]
    public async Task HandleAsync_WhenEmpty_Succeeds()
    {
        await _handler.HandleAsync(TestContext.Current.CancellationToken);

        Assert.Empty(_store.Items);
        Assert.Empty(_events.Published);
    }
}
