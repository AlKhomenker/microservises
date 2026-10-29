using TodoApp.Modules.AuditLogs.Application.Abstractions;
using TodoApp.Modules.AuditLogs.Application.Contracts;

namespace TodoApp.Modules.AuditLogs.Application.GetAuditLogs;

public sealed class GetAuditLogsHandler(IAuditLogRepository repository) // primary-конструктор с репозиторием журнала
{
    /* --- Methods --- */
    /// <summary>Returns all audit log entries, newest first.</summary>
    public async Task<IReadOnlyList<AuditLogEntryDto>> HandleAsync(CancellationToken cancellationToken) // читает все записи и возвращает их списком AuditLogEntryDto
    {
        var entries = await repository.GetAllAsync(cancellationToken);

        return entries.Select(AuditLogEntryDto.FromDomain).ToList();
    }
}
