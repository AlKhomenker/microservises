using TodoApp.Modules.AuditLogs.Application.Abstractions;
using TodoApp.Modules.AuditLogs.Domain;

namespace TodoApp.Modules.AuditLogs.UnitTests.Fakes;

/// <summary>Test double that keeps entries in memory and counts commits.</summary>
internal sealed class InMemoryAuditLogStore : IAuditLogRepository, IAuditLogsUnitOfWork
{
    private readonly List<AuditLogEntry> _pending = [];
    private readonly List<AuditLogEntry> _removed = [];

    public List<AuditLogEntry> Entries { get; } = [];

    public int SaveChangesCalls { get; private set; }

    public Task<IReadOnlyList<AuditLogEntry>> GetAllAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<AuditLogEntry>>(
            Entries.OrderByDescending(e => e.Date).ThenByDescending(e => e.Id).ToList());

    public void Add(AuditLogEntry entry) => _pending.Add(entry);

    public void Remove(AuditLogEntry entry) => _removed.Add(entry);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var changes = _pending.Count + _removed.Count;

        Entries.AddRange(_pending);
        Entries.RemoveAll(_removed.Contains);
        _pending.Clear();
        _removed.Clear();
        SaveChangesCalls++;

        return Task.FromResult(changes);
    }
}
