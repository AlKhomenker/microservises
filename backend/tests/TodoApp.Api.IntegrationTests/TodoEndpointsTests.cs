using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;

namespace TodoApp.Api.IntegrationTests;

public sealed class TodoEndpointsTests(TodoApiFactory factory) : IClassFixture<TodoApiFactory>, IAsyncLifetime
{
    private const string Url = "/api/todos";

    private readonly HttpClient _client = factory.CreateClient();

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
        var todos = await _client.GetFromJsonAsync<TodoResponse[]>(Url, Ct);

        Assert.NotNull(todos);
        Assert.Empty(todos);
    }

    [Fact]
    public async Task Post_WithValidText_Returns201AndItem()
    {
        var response = await _client.PostAsJsonAsync(Url, new { text = "  Buy milk  " }, Ct);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var todo = await response.Content.ReadFromJsonAsync<TodoResponse>(Ct);
        Assert.NotNull(todo);
        Assert.NotEqual(Guid.Empty, todo.Id);
        Assert.Equal("Buy milk", todo.Text);
        Assert.Equal(factory.Clock.GetUtcNow(), todo.CreateDate);
    }

    [Fact]
    public async Task Post_ThenGetAll_ReturnsItemsNewestFirst()
    {
        await CreateAsync("first");
        factory.Clock.Advance(TimeSpan.FromMinutes(1));
        await CreateAsync("second");

        var todos = await _client.GetFromJsonAsync<TodoResponse[]>(Url, Ct);

        Assert.NotNull(todos);
        Assert.Equal(["second", "first"], todos.Select(t => t.Text));
    }

    [Fact]
    public async Task Post_ThenGetAll_ReturnsSameCreateDateWithoutPrecisionLoss()
    {
        factory.Clock.Advance(TimeSpan.FromTicks(1234567));
        var created = await CreateAsync("precise");

        var todos = await _client.GetFromJsonAsync<TodoResponse[]>(Url, Ct);

        Assert.NotNull(todos);
        Assert.Equal(created.CreateDate, Assert.Single(todos).CreateDate);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Post_WithEmptyText_Returns400ValidationProblem(string text)
    {
        var response = await _client.PostAsJsonAsync(Url, new { text }, Ct);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(Ct);
        Assert.NotNull(problem);
        Assert.Contains("text", problem.Errors.Keys);
    }

    [Fact]
    public async Task Post_WithTooLongText_Returns400()
    {
        var response = await _client.PostAsJsonAsync(Url, new { text = new string('x', 501) }, Ct);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Delete_ExistingItem_Returns204AndRemovesIt()
    {
        var created = await CreateAsync("to delete");

        var response = await _client.DeleteAsync($"{Url}/{created.Id}", Ct);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var todos = await _client.GetFromJsonAsync<TodoResponse[]>(Url, Ct);
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
    public async Task Delete_WithInvalidId_Returns404()
    {
        var response = await _client.DeleteAsync($"{Url}/not-a-guid", Ct);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteAll_Returns204AndRemovesEveryItem()
    {
        await CreateAsync("first");
        await CreateAsync("second");

        var response = await _client.DeleteAsync(Url, Ct);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var todos = await _client.GetFromJsonAsync<TodoResponse[]>(Url, Ct);
        Assert.NotNull(todos);
        Assert.Empty(todos);
    }

    [Fact]
    public async Task DeleteAll_WhenEmpty_Returns204()
    {
        var response = await _client.DeleteAsync(Url, Ct);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Theory]
    [InlineData("/health/live")]
    [InlineData("/health/ready")]
    public async Task HealthChecks_ReturnHealthy(string path)
    {
        var response = await _client.GetAsync(path, Ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private async Task<TodoResponse> CreateAsync(string text)
    {
        var response = await _client.PostAsJsonAsync(Url, new { text }, Ct);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<TodoResponse>(Ct))!;
    }

    private sealed record TodoResponse(Guid Id, string Text, DateTimeOffset CreateDate);
}
