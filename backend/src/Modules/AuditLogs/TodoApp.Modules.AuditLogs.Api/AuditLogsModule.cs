using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TodoApp.Modules.AuditLogs.Api.Endpoints;
using TodoApp.Modules.AuditLogs.Application;
using TodoApp.Modules.AuditLogs.Infrastructure;
using TodoApp.Shared.Abstractions.Modules;

namespace TodoApp.Modules.AuditLogs.Api;

public sealed class AuditLogsModule : IModule
{
    /* --- Properties --- */
    public string Name => "AuditLogs"; // имя модуля

    /* --- Methods --- */
    public void RegisterServices(IServiceCollection services, IConfiguration configuration) // регистрирует в DI сервисы Application и Infrastructure
    {
        services.AddAuditLogsApplication();
        services.AddAuditLogsInfrastructure(configuration);
    }
    public void MapEndpoints(IEndpointRouteBuilder endpoints) => endpoints.MapAuditLogEndpoints(); // добавляет HTTP endpoints журнала
}
