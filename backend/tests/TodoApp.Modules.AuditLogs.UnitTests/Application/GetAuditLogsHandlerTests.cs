using TodoApp.Modules.AuditLogs.Application.GetAuditLogs;
using TodoApp.Modules.AuditLogs.Domain;
using TodoApp.Modules.AuditLogs.UnitTests.Fakes;

namespace TodoApp.Modules.AuditLogs.UnitTests.Application;

public sealed class GetAuditLogsHandlerTests
{
    private readonly InMemoryAuditLogStore _store = new();
    private readonly GetAuditLogsHandler _handler;

    public GetAuditLogsHandlerTests() => _handler = new GetAuditLogsHandler(_store);

    [Fact]
    public async Task HandleAsync_WhenEmpty_ReturnsEmptyList()
    {
        var result = await _handler.HandleAsync(TestContext.Current.CancellationToken);

        Assert.Empty(result);
    }

    [Fact]
    public async Task HandleAsync_MapsAllFieldsNewestFirst()
    {
        var older = AuditLogEntry.Create(AuditLogType.Add, "todos", Guid.NewGuid(), new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero));
        var newer = AuditLogEntry.Create(AuditLogType.Removed, "temporary-todos", Guid.NewGuid(), new DateTimeOffset(2026, 2, 1, 0, 0, 0, TimeSpan.Zero));
        _store.Entries.AddRange([older, newer]);

        var result = await _handler.HandleAsync(TestContext.Current.CancellationToken);

        Assert.Collection(
            result,
            first => Assert.Equal(
                (newer.Id, newer.Type, newer.Date, newer.ListName, newer.ItemId),
                (first.Id, first.Type, first.Date, first.ListName, first.ItemId)),
            second => Assert.Equal(
                (older.Id, older.Type, older.Date, older.ListName, older.ItemId),
                (second.Id, second.Type, second.Date, second.ListName, second.ItemId)));
    }
}
