using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using TodoApp.Shared.Kernel.Events;

namespace TodoApp.AuditLogs.IntegrationTests;

/// <summary>
/// The list modules run in other services, so events are published here
/// through the host's own IEventPublisher, the same way a list module does it.
/// </summary>
public sealed class AuditLogEndpointsTests(AuditLogsApiFactory factory) : IClassFixture<AuditLogsApiFactory>, IAsyncLifetime
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
    public async Task ItemAdded_WritesAddEntry()
    {
        var itemId = Guid.NewGuid();

        await PublishAsync("todos", itemId, ListItemChangeType.Added);

        var entry = Assert.Single(await GetEntriesAsync());
        Assert.Equal("add", entry.Type);
        Assert.Equal(Today, entry.Date);
        Assert.Equal("todos", entry.ListName);
        Assert.Equal(itemId, entry.ItemId);
    }

    [Fact]
    public async Task ItemAddedThenRemoved_WritesAddAndRemovedEntries()
    {
        var itemId = Guid.NewGuid();

        await PublishAsync("todos", itemId, ListItemChangeType.Added);
        await PublishAsync("todos", itemId, ListItemChangeType.Removed);

        var entries = await GetEntriesAsync();
        Assert.Equal(["add", "removed"], entries.Select(e => e.Type).Order());
        Assert.All(entries, e => Assert.Equal(itemId, e.ItemId));
    }

    [Fact]
    public async Task Events_KeepTheirListName()
    {
        await PublishAsync("other-list", Guid.NewGuid(), ListItemChangeType.Added);

        var entry = Assert.Single(await GetEntriesAsync());
        Assert.Equal("other-list", entry.ListName);
    }

    [Fact]
    public async Task DeleteAll_Returns204AndRemovesEveryEntry()
    {
        await PublishAsync("todos", Guid.NewGuid(), ListItemChangeType.Added);

        var response = await _client.DeleteAsync(Url, Ct);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Empty(await GetEntriesAsync());
    }

    [Theory]
    [InlineData("/health/live")]
    [InlineData("/health/ready")]
    public async Task HealthChecks_ReturnHealthy(string path)
    {
        var response = await _client.GetAsync(path, Ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private async Task PublishAsync(string listName, Guid itemId, ListItemChangeType changeType)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var publisher = scope.ServiceProvider.GetRequiredService<IEventPublisher>();
        await publisher.PublishAsync(new ListItemChanged(listName, itemId, changeType, factory.Clock.GetUtcNow()), Ct);
    }

    private async Task<AuditLogResponse[]> GetEntriesAsync() =>
        (await _client.GetFromJsonAsync<AuditLogResponse[]>(Url, Ct))!;

    private sealed record AuditLogResponse(Guid Id, string Type, DateOnly Date, string ListName, Guid ItemId);
}
