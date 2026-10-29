using Microsoft.Extensions.Time.Testing;
using TodoApp.Modules.TemporaryTodos.Application.GetTemporaryTodos;
using TodoApp.Modules.TemporaryTodos.Domain;
using TodoApp.Modules.TemporaryTodos.UnitTests.Fakes;

namespace TodoApp.Modules.TemporaryTodos.UnitTests.Application;

public sealed class GetTemporaryTodosHandlerTests
{
    private static readonly DateOnly Today = new(2026, 10, 21);

    private readonly InMemoryTemporaryTodoStore _store = new();
    private readonly FakeTimeProvider _clock = new(new DateTimeOffset(2026, 10, 21, 9, 30, 0, TimeSpan.Zero));
    private readonly GetTemporaryTodosHandler _handler;

    public GetTemporaryTodosHandlerTests() => _handler = new GetTemporaryTodosHandler(_store, _clock);

    [Fact]
    public async Task HandleAsync_WhenEmpty_ReturnsEmptyList()
    {
        var result = await _handler.HandleAsync(TestContext.Current.CancellationToken);

        Assert.Empty(result);
    }

    [Fact]
    public async Task HandleAsync_MapsAllFieldsWithStatusForToday()
    {
        var expired = TemporaryTodoItem.Create("expired", Today.AddDays(-1), _clock.GetUtcNow()).MyValue;
        var active = TemporaryTodoItem.Create("active", Today, _clock.GetUtcNow()).MyValue;
        _store.Items.AddRange([expired, active]);

        var result = await _handler.HandleAsync(TestContext.Current.CancellationToken);

        Assert.Collection(
            result,
            first => Assert.Equal(
                (active.Id, active.Text, active.StartDate, TemporaryTodoStatus.Active),
                (first.Id, first.Text, first.StartDate, first.Status)),
            second => Assert.Equal(
                (expired.Id, expired.Text, expired.StartDate, TemporaryTodoStatus.Expired),
                (second.Id, second.Text, second.StartDate, second.Status)));
    }

    [Fact]
    public async Task HandleAsync_AfterStartDatePasses_ReturnsExpired()
    {
        _store.Items.Add(TemporaryTodoItem.Create("today", Today, _clock.GetUtcNow()).MyValue);
        _clock.Advance(TimeSpan.FromDays(1));

        var result = await _handler.HandleAsync(TestContext.Current.CancellationToken);

        Assert.Equal(TemporaryTodoStatus.Expired, Assert.Single(result).Status);
    }
}
