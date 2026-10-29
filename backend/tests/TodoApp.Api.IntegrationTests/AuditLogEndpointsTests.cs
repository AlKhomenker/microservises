using System.Net;
using System.Net.Http.Json;

namespace TodoApp.Api.IntegrationTests;

public sealed class AuditLogEndpointsTests(TodoApiFactory factory) : IClassFixture<TodoApiFactory>, IAsyncLifetime
{
    private const string Url = "/api/audit-logs";

    private readonly HttpClient _client = factory.CreateClient();

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private DateOnly Today => DateOnly.FromDateTime(factory.Clock.GetUtcNow().UtcDateTime);

    public async ValueTask InitializeAsync() => await factory.ResetDatabaseAsync();

    public ValueTask DisposeAsync()
    {
        _client.Dispose();
        return ValueTask.CompletedTask;
    }

    [Fact]
    public async Task GetAll_WhenEmpty_ReturnsEmptyArray()
    {
        var entries = await GetEntriesAsync();

        Assert.Empty(entries);
    }

    [Fact]
    public async Task AddingTodo_WritesAddEntry()
    {
        var todo = await PostAsync<ItemResponse>("/api/todos", new { text = "Buy milk" });

        var entry = Assert.Single(await GetEntriesAsync());
        Assert.Equal("add", entry.Type);
        Assert.Equal(Today, entry.Date);
        Assert.Equal("todos", entry.ListName);
        Assert.Equal(todo.Id, entry.ItemId);
    }

    [Fact]
    public async Task DeletingTodo_WritesRemovedEntry()
    {
        var todo = await PostAsync<ItemResponse>("/api/todos", new { text = "Buy milk" });

        (await _client.DeleteAsync($"/api/todos/{todo.Id}", Ct)).EnsureSuccessStatusCode();

        var entries = await GetEntriesAsync();
        Assert.Equal(["add", "removed"], entries.Select(e => e.Type).Order());
        Assert.All(entries, e => Assert.Equal(todo.Id, e.ItemId));
    }

    [Fact]
    public async Task TemporaryTodos_WriteEntriesWithTheirListName()
    {
        var todo = await PostAsync<ItemResponse>("/api/temporary-todos", new { text = "Call Bob", startDate = Today });
        (await _client.DeleteAsync($"/api/temporary-todos/{todo.Id}", Ct)).EnsureSuccessStatusCode();

        var entries = await GetEntriesAsync();
        Assert.Equal(["add", "removed"], entries.Select(e => e.Type).Order());
        Assert.All(entries, e => Assert.Equal("temporary-todos", e.ListName));
    }

    [Fact]
    public async Task RemovingAllTodos_WritesOneRemovedEntryPerItem()
    {
        await PostAsync<ItemResponse>("/api/todos", new { text = "first" });
        await PostAsync<ItemResponse>("/api/todos", new { text = "second" });

        (await _client.DeleteAsync("/api/todos", Ct)).EnsureSuccessStatusCode();

        var entries = await GetEntriesAsync();
        Assert.Equal(2, entries.Count(e => e.Type == "removed"));
    }

    [Fact]
    public async Task FailedAction_WritesNoEntry()
    {
        await _client.PostAsJsonAsync("/api/todos", new { text = "" }, Ct);
        await _client.DeleteAsync($"/api/todos/{Guid.NewGuid()}", Ct);

        Assert.Empty(await GetEntriesAsync());
    }

    [Fact]
    public async Task DeleteAll_Returns204AndRemovesEveryEntry()
    {
        await PostAsync<ItemResponse>("/api/todos", new { text = "first" });

        var response = await _client.DeleteAsync(Url, Ct);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Empty(await GetEntriesAsync());
        var todos = await _client.GetFromJsonAsync<object[]>("/api/todos", Ct);
        Assert.NotNull(todos);
        Assert.Single(todos);
    }

    private async Task<T> PostAsync<T>(string url, object body)
    {
        var response = await _client.PostAsJsonAsync(url, body, Ct);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<T>(Ct))!;
    }

    private async Task<AuditLogResponse[]> GetEntriesAsync() =>
        (await _client.GetFromJsonAsync<AuditLogResponse[]>(Url, Ct))!;

    private sealed record ItemResponse(Guid Id);

    private sealed record AuditLogResponse(Guid Id, string Type, DateOnly Date, string ListName, Guid ItemId);
}
