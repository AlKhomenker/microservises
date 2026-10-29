using TodoApp.Modules.Todos.Application.DeleteTodo;
using Microsoft.Extensions.Time.Testing;
using TodoApp.Modules.Todos.Domain;
using TodoApp.Modules.Todos.UnitTests.Fakes;
using TodoApp.Shared.Kernel;
using TodoApp.Shared.Kernel.Events;

namespace TodoApp.Modules.Todos.UnitTests.Application;

public sealed class DeleteTodoHandlerTests
{
    private readonly InMemoryTodoStore _store = new();
    private readonly FakeTimeProvider _clock = new(new DateTimeOffset(2026, 10, 21, 9, 30, 0, TimeSpan.Zero));
    private readonly FakeEventPublisher _events = new();
    private readonly DeleteTodoHandler _handler;

    public DeleteTodoHandlerTests() => _handler = new DeleteTodoHandler(_store, _store, _clock, _events);

    [Fact]
    public async Task HandleAsync_WithExistingId_RemovesItem()
    {
        var item = TodoItem.Create("Delete me", DateTimeOffset.UtcNow).MyValue;
        _store.Items.Add(item);

        var result = await _handler.HandleAsync(item.Id, TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Empty(_store.Items);
        Assert.Equal(1, _store.SaveChangesCalls);
        Assert.Equal(
            new ListItemChanged("todos", item.Id, ListItemChangeType.Removed, _clock.GetUtcNow()),
            Assert.Single(_events.Published));
    }

    [Fact]
    public async Task HandleAsync_WithUnknownId_ReturnsNotFound()
    {
        var id = Guid.NewGuid();

        var result = await _handler.HandleAsync(id, TestContext.Current.CancellationToken);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
        Assert.Equal(TodoErrors.NotFound(id), result.Error);
        Assert.Equal(0, _store.SaveChangesCalls);
        Assert.Empty(_events.Published);
    }
}
