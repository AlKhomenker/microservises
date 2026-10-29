using Microsoft.Extensions.Time.Testing;
using TodoApp.Modules.TemporaryTodos.Application.DeleteTemporaryTodo;
using TodoApp.Modules.TemporaryTodos.Domain;
using TodoApp.Modules.TemporaryTodos.UnitTests.Fakes;
using TodoApp.Shared.Kernel;
using TodoApp.Shared.Kernel.Events;

namespace TodoApp.Modules.TemporaryTodos.UnitTests.Application;

public sealed class DeleteTemporaryTodoHandlerTests
{
    private readonly InMemoryTemporaryTodoStore _store = new();
    private readonly FakeTimeProvider _clock = new(new DateTimeOffset(2026, 10, 21, 9, 30, 0, TimeSpan.Zero));
    private readonly FakeEventPublisher _events = new();
    private readonly DeleteTemporaryTodoHandler _handler;

    public DeleteTemporaryTodoHandlerTests() => _handler = new DeleteTemporaryTodoHandler(_store, _store, _clock, _events);

    [Fact]
    public async Task HandleAsync_WithExistingId_RemovesItem()
    {
        var item = TemporaryTodoItem.Create("Delete me", new DateOnly(2026, 10, 21), DateTimeOffset.UtcNow).MyValue;
        _store.Items.Add(item);

        var result = await _handler.HandleAsync(item.Id, TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Empty(_store.Items);
        Assert.Equal(1, _store.SaveChangesCalls);
        Assert.Equal(
            new ListItemChanged("temporary-todos", item.Id, ListItemChangeType.Removed, _clock.GetUtcNow()),
            Assert.Single(_events.Published));
    }

    [Fact]
    public async Task HandleAsync_WithUnknownId_ReturnsNotFound()
    {
        var id = Guid.NewGuid();

        var result = await _handler.HandleAsync(id, TestContext.Current.CancellationToken);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
        Assert.Equal(TemporaryTodoErrors.NotFound(id), result.Error);
        Assert.Equal(0, _store.SaveChangesCalls);
        Assert.Empty(_events.Published);
    }
}
