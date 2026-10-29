using TodoApp.Modules.AuditLogs.Domain;

namespace TodoApp.Modules.AuditLogs.Application.Abstractions;

public interface IAuditLogRepository
{
    /* --- Methods --- */
    /// <summary>Returns all entries, newest first.</summary>
    Task<IReadOnlyList<AuditLogEntry>> GetAllAsync(CancellationToken cancellationToken); // возвращает все записи журнала, сначала новые
    void Add(AuditLogEntry entry); // добавляет запись; сохраняется при SaveChangesAsync
    void Remove(AuditLogEntry entry); // удаляет запись; сохраняется при SaveChangesAsync
}
