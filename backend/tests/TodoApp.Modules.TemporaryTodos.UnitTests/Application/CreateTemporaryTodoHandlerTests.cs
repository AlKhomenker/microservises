using Microsoft.Extensions.Time.Testing;
using TodoApp.Modules.TemporaryTodos.Application.CreateTemporaryTodo;
using TodoApp.Modules.TemporaryTodos.Domain;
using TodoApp.Modules.TemporaryTodos.UnitTests.Fakes;
using TodoApp.Shared.Kernel.Events;

namespace TodoApp.Modules.TemporaryTodos.UnitTests.Application;

public sealed class CreateTemporaryTodoHandlerTests
{
    private static readonly DateOnly Today = new(2026, 10, 21);

    private readonly InMemoryTemporaryTodoStore _store = new();
    private readonly FakeTimeProvider _clock = new(new DateTimeOffset(2026, 10, 21, 9, 30, 0, TimeSpan.Zero));
    private readonly FakeEventPublisher _events = new();
    private readonly CreateTemporaryTodoHandler _handler;

    public CreateTemporaryTodoHandlerTests() => _handler = new CreateTemporaryTodoHandler(_store, _store, _clock, _events);

    [Fact]
    public async Task HandleAsync_WithValidInput_SavesItemAndReturnsActive()
    {
        var result = await _handler.HandleAsync(
            new CreateTemporaryTodoCommand("Write tests", Today), TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal("Write tests", result.MyValue.Text);
        Assert.Equal(Today, result.MyValue.StartDate);
        Assert.Equal(TemporaryTodoStatus.Active, result.MyValue.Status);

        var saved = Assert.Single(_store.Items);
        Assert.Equal(result.MyValue.Id, saved.Id);
        Assert.Equal(1, _store.SaveChangesCalls);
        Assert.Equal(
            new ListItemChanged("temporary-todos", saved.Id, ListItemChangeType.Added, _clock.GetUtcNow()),
            Assert.Single(_events.Published));
    }

    [Fact]
    public async Task HandleAsync_WithPastStartDate_ReturnsExpired()
    {
        var result = await _handler.HandleAsync(
            new CreateTemporaryTodoCommand("Yesterday", Today.AddDays(-1)), TestContext.Current.CancellationToken);

        Assert.Equal(TemporaryTodoStatus.Expired, result.MyValue.Status);
    }

    [Fact]
    public async Task HandleAsync_WithInvalidText_ReturnsErrorAndSavesNothing()
    {
        var result = await _handler.HandleAsync(
            new CreateTemporaryTodoCommand(" ", Today), TestContext.Current.CancellationToken);

        Assert.False(result.IsSuccess);
        Assert.Equal(TemporaryTodoErrors.TextRequired, result.Error);
        Assert.Empty(_store.Items);
        Assert.Equal(0, _store.SaveChangesCalls);
        Assert.Empty(_events.Published);
    }

    [Fact]
    public async Task HandleAsync_WithoutStartDate_ReturnsErrorAndSavesNothing()
    {
        var result = await _handler.HandleAsync(
            new CreateTemporaryTodoCommand("Write tests", null), TestContext.Current.CancellationToken);

        Assert.False(result.IsSuccess);
        Assert.Equal(TemporaryTodoErrors.StartDateRequired, result.Error);
        Assert.Empty(_store.Items);
        Assert.Equal(0, _store.SaveChangesCalls);
        Assert.Empty(_events.Published);
    }
}
