using TodoApp.Modules.AuditLogs.Application.Abstractions;

namespace TodoApp.Modules.AuditLogs.Application.DeleteAllAuditLogs;

public sealed class DeleteAllAuditLogsHandler(IAuditLogRepository repository, IAuditLogsUnitOfWork unitOfWork) // primary-конструктор с репозиторием журнала и unit of work
{
    /* --- Methods --- */
    /// <summary>Deletes every audit log entry. Succeeds when the log is already empty.</summary>
    public async Task HandleAsync(CancellationToken cancellationToken) // удаляет все записи журнала и сохраняет изменения
    {
        var entries = await repository.GetAllAsync(cancellationToken);
        foreach (var entry in entries)
        {
            repository.Remove(entry);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
