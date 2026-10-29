using TodoApp.Modules.AuditLogs.Application.DeleteAllAuditLogs;
using TodoApp.Modules.AuditLogs.Domain;
using TodoApp.Modules.AuditLogs.UnitTests.Fakes;

namespace TodoApp.Modules.AuditLogs.UnitTests.Application;

public sealed class DeleteAllAuditLogsHandlerTests
{
    private readonly InMemoryAuditLogStore _store = new();
    private readonly DeleteAllAuditLogsHandler _handler;

    public DeleteAllAuditLogsHandlerTests() => _handler = new DeleteAllAuditLogsHandler(_store, _store);

    [Fact]
    public async Task HandleAsync_WithEntries_RemovesAllInOneCommit()
    {
        _store.Entries.Add(AuditLogEntry.Create(AuditLogType.Add, "todos", Guid.NewGuid(), DateTimeOffset.UtcNow));
        _store.Entries.Add(AuditLogEntry.Create(AuditLogType.Removed, "todos", Guid.NewGuid(), DateTimeOffset.UtcNow));

        await _handler.HandleAsync(TestContext.Current.CancellationToken);

        Assert.Empty(_store.Entries);
        Assert.Equal(1, _store.SaveChangesCalls);
    }

    [Fact]
    public async Task HandleAsync_WhenEmpty_Succeeds()
    {
        await _handler.HandleAsync(TestContext.Current.CancellationToken);

        Assert.Empty(_store.Entries);
    }
}
