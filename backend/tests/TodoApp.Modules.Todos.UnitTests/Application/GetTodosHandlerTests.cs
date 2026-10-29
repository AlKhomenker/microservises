using TodoApp.Modules.Todos.Application.GetTodos;
using TodoApp.Modules.Todos.Domain;
using TodoApp.Modules.Todos.UnitTests.Fakes;

namespace TodoApp.Modules.Todos.UnitTests.Application;

public sealed class GetTodosHandlerTests
{
    private readonly InMemoryTodoStore _store = new();
    private readonly GetTodosHandler _handler;

    public GetTodosHandlerTests() => _handler = new GetTodosHandler(_store);

    [Fact]
    public async Task HandleAsync_WhenEmpty_ReturnsEmptyList()
    {
        var result = await _handler.HandleAsync(TestContext.Current.CancellationToken);

        Assert.Empty(result);
    }

    [Fact]
    public async Task HandleAsync_MapsAllFieldsInRepositoryOrder()
    {
        var older = TodoItem.Create("older", new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero)).MyValue;
        var newer = TodoItem.Create("newer", new DateTimeOffset(2026, 2, 1, 0, 0, 0, TimeSpan.Zero)).MyValue;
        _store.Items.AddRange([older, newer]);

        var result = await _handler.HandleAsync(TestContext.Current.CancellationToken);

        Assert.Collection(
            result,
            first => Assert.Equal((newer.Id, newer.Text, newer.CreateDate), (first.Id, first.Text, first.CreateDate)),
            second => Assert.Equal((older.Id, older.Text, older.CreateDate), (second.Id, second.Text, second.CreateDate)));
    }
}
