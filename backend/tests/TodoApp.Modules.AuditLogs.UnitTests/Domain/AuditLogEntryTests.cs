using TodoApp.Modules.AuditLogs.Domain;

namespace TodoApp.Modules.AuditLogs.UnitTests.Domain;

public sealed class AuditLogEntryTests
{
    [Fact]
    public void Create_SetsAllFields()
    {
        var itemId = Guid.NewGuid();

        var entry = AuditLogEntry.Create(AuditLogType.Add, "todos", itemId, new DateTimeOffset(2026, 10, 21, 12, 0, 0, TimeSpan.Zero));

        Assert.NotEqual(Guid.Empty, entry.Id);
        Assert.Equal(AuditLogType.Add, entry.Type);
        Assert.Equal(new DateOnly(2026, 10, 21), entry.Date);
        Assert.Equal("todos", entry.ListName);
        Assert.Equal(itemId, entry.ItemId);
    }

    [Fact]
    public void Create_UsesUtcDate()
    {
        // 2026-10-22 01:00 at UTC+3 is still 2026-10-21 in UTC.
        var entry = AuditLogEntry.Create(
            AuditLogType.Removed, "todos", Guid.NewGuid(), new DateTimeOffset(2026, 10, 22, 1, 0, 0, TimeSpan.FromHours(3)));

        Assert.Equal(new DateOnly(2026, 10, 21), entry.Date);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithoutListName_Throws(string listName)
    {
        Assert.Throws<ArgumentException>(() =>
            AuditLogEntry.Create(AuditLogType.Add, listName, Guid.NewGuid(), DateTimeOffset.UtcNow));
    }
}
