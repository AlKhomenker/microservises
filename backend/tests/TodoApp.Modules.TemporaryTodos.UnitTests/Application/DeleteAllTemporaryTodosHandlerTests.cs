using Microsoft.Extensions.Time.Testing;
using TodoApp.Modules.TemporaryTodos.Application.DeleteAllTemporaryTodos;
using TodoApp.Modules.TemporaryTodos.Domain;
using TodoApp.Modules.TemporaryTodos.UnitTests.Fakes;
using TodoApp.Shared.Kernel.Events;

namespace TodoApp.Modules.TemporaryTodos.UnitTests.Application;

public sealed class DeleteAllTemporaryTodosHandlerTests
{
    private readonly InMemoryTemporaryTodoStore _store = new();
    private readonly FakeTimeProvider _clock = new(new DateTimeOffset(2026, 10, 21, 9, 30, 0, TimeSpan.Zero));
    private readonly FakeEventPublisher _events = new();
    private readonly DeleteAllTemporaryTodosHandler _handler;

    public DeleteAllTemporaryTodosHandlerTests() => _handler = new DeleteAllTemporaryTodosHandler(_store, _store, _clock, _events);

    [Fact]
    public async Task HandleAsync_WithItems_RemovesAllInOneCommit()
    {
        var date = new DateOnly(2026, 10, 21);
        var first = TemporaryTodoItem.Create("first", date, DateTimeOffset.UtcNow).MyValue;
        var second = TemporaryTodoItem.Create("second", date, DateTimeOffset.UtcNow).MyValue;
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
