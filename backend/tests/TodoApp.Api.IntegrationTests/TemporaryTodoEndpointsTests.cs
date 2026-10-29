using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;

namespace TodoApp.Api.IntegrationTests;

public sealed class TemporaryTodoEndpointsTests(TodoApiFactory factory) : IClassFixture<TodoApiFactory>, IAsyncLifetime
{
    private const string Url = "/api/temporary-todos";

    private readonly HttpClient _client = factory.CreateClient();

    // Tests in this class share the factory clock, and some move it forward, so "today" is read from it.
    private DateOnly Today => DateOnly.FromDateTime(factory.Clock.GetUtcNow().UtcDateTime);

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync() => await factory.ResetDatabaseAsync();

    public ValueTask DisposeAsync()
    {
        _client.Dispose();
        return ValueTask.CompletedTask;
    }

    [Fact]
    public async Task GetAll_WhenEmpty_ReturnsEmptyArray()
    {
        var todos = await _client.GetFromJsonAsync<TemporaryTodoResponse[]>(Url, Ct);

        Assert.NotNull(todos);
        Assert.Empty(todos);
    }

    [Fact]
    public async Task Post_WithValidInput_Returns201AndActiveItem()
    {
        var response = await _client.PostAsJsonAsync(Url, new { text = "  Buy milk  ", startDate = Today }, Ct);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var todo = await response.Content.ReadFromJsonAsync<TemporaryTodoResponse>(Ct);
        Assert.NotNull(todo);
        Assert.NotEqual(Guid.Empty, todo.Id);
        Assert.Equal("Buy milk", todo.Text);
        Assert.Equal(Today, todo.StartDate);
        Assert.Equal("active", todo.Status);
    }

    [Fact]
    public async Task Post_WithPastStartDate_ReturnsExpiredItem()
    {
        var created = await CreateAsync("yesterday", Today.AddDays(-1));

        Assert.Equal("expired", created.Status);
    }

    [Fact]
    public async Task GetAll_AfterStartDatePasses_ReturnsExpired()
    {
        await CreateAsync("today", Today);
        factory.Clock.Advance(TimeSpan.FromDays(1));

        var todos = await _client.GetFromJsonAsync<TemporaryTodoResponse[]>(Url, Ct);

        Assert.NotNull(todos);
        Assert.Equal("expired", Assert.Single(todos).Status);
    }

    [Fact]
    public async Task Post_ThenGetAll_ReturnsItemsLatestStartDateFirst()
    {
        await CreateAsync("tomorrow", Today.AddDays(1));
        await CreateAsync("yesterday", Today.AddDays(-1));
        await CreateAsync("today", Today);

        var todos = await _client.GetFromJsonAsync<TemporaryTodoResponse[]>(Url, Ct);

        Assert.NotNull(todos);
        Assert.Equal(["tomorrow", "today", "yesterday"], todos.Select(t => t.Text));
        Assert.Equal(["active", "active", "expired"], todos.Select(t => t.Status));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Post_WithEmptyText_Returns400ValidationProblem(string text)
    {
        var response = await _client.PostAsJsonAsync(Url, new { text, startDate = Today }, Ct);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(Ct);
        Assert.NotNull(problem);
        Assert.Contains("text", problem.Errors.Keys);
    }

    [Fact]
    public async Task Post_WithoutStartDate_Returns400ValidationProblem()
    {
        var response = await _client.PostAsJsonAsync(Url, new { text = "Buy milk" }, Ct);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(Ct);
        Assert.NotNull(problem);
        Assert.Contains("startDate", problem.Errors.Keys);
    }

    [Fact]
    public async Task Post_WithTooLongText_Returns400()
    {
        var response = await _client.PostAsJsonAsync(Url, new { text = new string('x', 501), startDate = Today }, Ct);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Delete_ExistingItem_Returns204AndRemovesIt()
    {
        var created = await CreateAsync("to delete", Today);

        var response = await _client.DeleteAsync($"{Url}/{created.Id}", Ct);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var todos = await _client.GetFromJsonAsync<TemporaryTodoResponse[]>(Url, Ct);
        Assert.NotNull(todos);
        Assert.Empty(todos);
    }

    [Fact]
    public async Task Delete_UnknownItem_Returns404Problem()
    {
        var response = await _client.DeleteAsync($"{Url}/{Guid.NewGuid()}", Ct);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task DeleteAll_Returns204AndRemovesEveryItem()
    {
        await CreateAsync("first", Today);
        await CreateAsync("second", Today);

        var response = await _client.DeleteAsync(Url, Ct);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var todos = await _client.GetFromJsonAsync<TemporaryTodoResponse[]>(Url, Ct);
        Assert.NotNull(todos);
        Assert.Empty(todos);
    }

    [Fact]
    public async Task DeleteAll_DoesNotTouchRegularTodos()
    {
        var regular = await _client.PostAsJsonAsync("/api/todos", new { text = "regular" }, Ct);
        regular.EnsureSuccessStatusCode();
        await CreateAsync("temporary", Today);

        await _client.DeleteAsync(Url, Ct);

        var todos = await _client.GetFromJsonAsync<object[]>("/api/todos", Ct);
        Assert.NotNull(todos);
        Assert.Single(todos);
    }

    private async Task<TemporaryTodoResponse> CreateAsync(string text, DateOnly startDate)
    {
        var response = await _client.PostAsJsonAsync(Url, new { text, startDate }, Ct);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<TemporaryTodoResponse>(Ct))!;
    }

    private sealed record TemporaryTodoResponse(Guid Id, string Text, DateOnly StartDate, string Status);
}
