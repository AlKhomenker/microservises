using Microsoft.EntityFrameworkCore;
using TodoApp.Modules.AuditLogs.Application.Abstractions;
using TodoApp.Modules.AuditLogs.Domain;

namespace TodoApp.Modules.AuditLogs.Infrastructure.Persistence;

internal sealed class AuditLogRepository(AuditLogsDbContext dbContext) : IAuditLogRepository // primary-конструктор с DbContext модуля AuditLogs
{
    /* --- Methods --- */
    public async Task<IReadOnlyList<AuditLogEntry>> GetAllAsync(CancellationToken cancellationToken) => // возвращает все записи, сначала новые (Id - Guid v7, растёт со временем), без отслеживания изменений
        await dbContext.AuditLogEntries
            .AsNoTracking()
            .OrderByDescending(x => x.Date)
            .ThenByDescending(x => x.Id)
            .ToListAsync(cancellationToken);
    public void Add(AuditLogEntry entry) => dbContext.AuditLogEntries.Add(entry); // добавляет запись в DbContext
    public void Remove(AuditLogEntry entry) => dbContext.AuditLogEntries.Remove(entry); // удаляет запись из DbContext
}
