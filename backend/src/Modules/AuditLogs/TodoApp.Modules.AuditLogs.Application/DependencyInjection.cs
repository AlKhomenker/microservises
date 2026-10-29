using Microsoft.Extensions.DependencyInjection;
using TodoApp.Modules.AuditLogs.Application.DeleteAllAuditLogs;
using TodoApp.Modules.AuditLogs.Application.GetAuditLogs;
using TodoApp.Modules.AuditLogs.Application.RecordListItemChange;
using TodoApp.Shared.Kernel.Events;

namespace TodoApp.Modules.AuditLogs.Application;

public static class DependencyInjection
{
    /* --- Methods --- */
    public static IServiceCollection AddAuditLogsApplication(this IServiceCollection services) // регистрирует в DI обработчики Application и получателя ListItemChanged; возвращает services для цепочки вызовов
    {
        services.AddScoped<GetAuditLogsHandler>();
        services.AddScoped<DeleteAllAuditLogsHandler>();
        services.AddScoped<IIntegrationEventConsumer<ListItemChanged>, ListItemChangedConsumer>();

        return services;
    }
}
