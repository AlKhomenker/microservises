using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using TodoApp.Modules.AuditLogs.Application.Contracts;
using TodoApp.Modules.AuditLogs.Application.DeleteAllAuditLogs;
using TodoApp.Modules.AuditLogs.Application.GetAuditLogs;

namespace TodoApp.Modules.AuditLogs.Api.Endpoints;

internal static class AuditLogEndpoints
{
    /* --- Properties --- */
    public const string BasePath = "/api/audit-logs"; // базовый маршрут endpoints журнала

    /* --- Methods --- */
    public static IEndpointRouteBuilder MapAuditLogEndpoints(this IEndpointRouteBuilder endpoints) // добавляет маршруты GET и DELETE для журнала; возвращает endpoints для цепочки вызовов
    {
        var group = endpoints.MapGroup(BasePath).WithTags("AuditLogs");

        group.MapGet("/", GetAllAsync)
            .WithName("GetAuditLogs")
            .WithSummary("Returns all audit log entries, newest first.");

        group.MapDelete("/", DeleteAllAsync)
            .WithName("DeleteAllAuditLogs")
            .WithSummary("Deletes all audit log entries.")
            .Produces(StatusCodes.Status204NoContent);

        return endpoints;
    }

    private static async Task<Ok<IReadOnlyList<AuditLogEntryDto>>> GetAllAsync( // GET /api/audit-logs, возвращает 200 со всеми записями
        GetAuditLogsHandler handler,
        CancellationToken cancellationToken) =>
        TypedResults.Ok(await handler.HandleAsync(cancellationToken));

    private static async Task<NoContent> DeleteAllAsync( // DELETE /api/audit-logs, удаляет все записи и возвращает 204
        DeleteAllAuditLogsHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(cancellationToken);

        return TypedResults.NoContent();
    }
}
