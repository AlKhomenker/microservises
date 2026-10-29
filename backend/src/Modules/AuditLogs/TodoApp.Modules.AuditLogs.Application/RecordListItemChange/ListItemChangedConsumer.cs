using TodoApp.Modules.AuditLogs.Application.Abstractions;
using TodoApp.Modules.AuditLogs.Domain;
using TodoApp.Shared.Kernel.Events;

namespace TodoApp.Modules.AuditLogs.Application.RecordListItemChange;

/// <summary>Writes one audit log entry for every item added to or removed from any list.</summary>
public sealed class ListItemChangedConsumer(IAuditLogRepository repository, IAuditLogsUnitOfWork unitOfWork) // primary-конструктор с репозиторием журнала и unit of work
    : IIntegrationEventConsumer<ListItemChanged>
{
    /* --- Methods --- */
    public async Task HandleAsync(ListItemChanged integrationEvent, CancellationToken cancellationToken) // сохраняет запись журнала для события ListItemChanged
    {
        ArgumentNullException.ThrowIfNull(integrationEvent);

        var type = integrationEvent.ChangeType switch
        {
            ListItemChangeType.Added => AuditLogType.Add,
            ListItemChangeType.Removed => AuditLogType.Removed,
            _ => throw new ArgumentOutOfRangeException(nameof(integrationEvent), integrationEvent.ChangeType, "Unknown change type."),
        };

        repository.Add(AuditLogEntry.Create(type, integrationEvent.ListName, integrationEvent.ItemId, integrationEvent.OccurredAt));
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
