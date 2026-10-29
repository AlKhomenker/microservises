using TodoApp.Modules.AuditLogs.Application.RecordListItemChange;
using TodoApp.Modules.AuditLogs.Domain;
using TodoApp.Modules.AuditLogs.UnitTests.Fakes;
using TodoApp.Shared.Kernel.Events;

namespace TodoApp.Modules.AuditLogs.UnitTests.Application;

public sealed class ListItemChangedConsumerTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 21, 9, 30, 0, TimeSpan.Zero);

    private readonly InMemoryAuditLogStore _store = new();
    private readonly ListItemChangedConsumer _consumer;

    public ListItemChangedConsumerTests() => _consumer = new ListItemChangedConsumer(_store, _store);

    [Theory]
    [InlineData(ListItemChangeType.Added, AuditLogType.Add)]
    [InlineData(ListItemChangeType.Removed, AuditLogType.Removed)]
    public async Task HandleAsync_SavesEntryWithMappedType(ListItemChangeType changeType, AuditLogType expected)
    {
        var itemId = Guid.NewGuid();

        await _consumer.HandleAsync(new ListItemChanged("todos", itemId, changeType, Now), TestContext.Current.CancellationToken);

        var entry = Assert.Single(_store.Entries);
        Assert.Equal(expected, entry.Type);
        Assert.Equal(new DateOnly(2026, 10, 21), entry.Date);
        Assert.Equal("todos", entry.ListName);
        Assert.Equal(itemId, entry.ItemId);
        Assert.Equal(1, _store.SaveChangesCalls);
    }
}
