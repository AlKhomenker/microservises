using Microsoft.Extensions.Time.Testing;
using TodoApp.Modules.Todos.Application.CreateTodo;
using TodoApp.Modules.Todos.Domain;
using TodoApp.Modules.Todos.UnitTests.Fakes;
using TodoApp.Shared.Kernel.Events;

namespace TodoApp.Modules.Todos.UnitTests.Application;

public sealed class CreateTodoHandlerTests
{
    private readonly InMemoryTodoStore _store = new();
    private readonly FakeTimeProvider _clock = new(new DateTimeOffset(2026, 10, 21, 9, 30, 0, TimeSpan.Zero));
    private readonly FakeEventPublisher _events = new();
    private readonly CreateTodoHandler _handler;

    public CreateTodoHandlerTests() => _handler = new CreateTodoHandler(_store, _store, _clock, _events);

    [Fact]
    public async Task HandleAsync_WithValidText_SavesItemWithCurrentTime()
    {
        var result = await _handler.HandleAsync(new CreateTodoCommand("Write tests"), TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal("Write tests", result.MyValue.Text);
        Assert.Equal(_clock.GetUtcNow(), result.MyValue.CreateDate);

        var saved = Assert.Single(_store.Items);
        Assert.Equal(result.MyValue.Id, saved.Id);
        Assert.Equal(1, _store.SaveChangesCalls);
        Assert.Equal(
            new ListItemChanged("todos", saved.Id, ListItemChangeType.Added, _clock.GetUtcNow()),
            Assert.Single(_events.Published));
    }

    [Fact]
    public async Task HandleAsync_WithInvalidText_ReturnsErrorAndSavesNothing()
    {
        var result = await _handler.HandleAsync(new CreateTodoCommand(" "), TestContext.Current.CancellationToken);

        Assert.False(result.IsSuccess);
        Assert.Equal(TodoErrors.TextRequired, result.Error);
        Assert.Empty(_store.Items);
        Assert.Equal(0, _store.SaveChangesCalls);
        Assert.Empty(_events.Published);
    }
}
