using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using TodoApp.Modules.TemporaryTodos.Application.Contracts;
using TodoApp.Modules.TemporaryTodos.Application.CreateTemporaryTodo;
using TodoApp.Modules.TemporaryTodos.Application.DeleteAllTemporaryTodos;
using TodoApp.Modules.TemporaryTodos.Application.DeleteTemporaryTodo;
using TodoApp.Modules.TemporaryTodos.Application.GetTemporaryTodos;
using TodoApp.Shared.Abstractions.Http;

namespace TodoApp.Modules.TemporaryTodos.Api.Endpoints;

internal static class TemporaryTodoEndpoints
{
    /* --- Properties --- */
    public const string BasePath = "/api/temporary-todos"; // базовый маршрут endpoints временных задач

    /* --- Methods --- */
    public static IEndpointRouteBuilder MapTemporaryTodoEndpoints(this IEndpointRouteBuilder endpoints) // добавляет маршруты GET, POST и DELETE для временных задач; возвращает endpoints для цепочки вызовов
    {
        var group = endpoints.MapGroup(BasePath).WithTags("TemporaryTodos");

        group.MapGet("/", GetAllAsync)
            .WithName("GetTemporaryTodos")
            .WithSummary("Returns all temporary todo items with their status, latest start date first.");

        group.MapPost("/", CreateAsync)
            .WithName("CreateTemporaryTodo")
            .WithSummary("Creates a temporary todo item.")
            .Produces<TemporaryTodoItemDto>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        group.MapDelete("/{id:guid}", DeleteAsync)
            .WithName("DeleteTemporaryTodo")
            .WithSummary("Deletes a temporary todo item.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/", DeleteAllAsync)
            .WithName("DeleteAllTemporaryTodos")
            .WithSummary("Deletes all temporary todo items.")
            .Produces(StatusCodes.Status204NoContent);

        return endpoints;
    }

    private static async Task<Ok<IReadOnlyList<TemporaryTodoItemDto>>> GetAllAsync( // GET /api/temporary-todos, возвращает 200 со всеми временными задачами
        GetTemporaryTodosHandler handler,
        CancellationToken cancellationToken) =>
        TypedResults.Ok(await handler.HandleAsync(cancellationToken));

    private static async Task<IResult> CreateAsync( // POST /api/temporary-todos, возвращает 201 с новой временной задачей или 400 при ошибке валидации
        CreateTemporaryTodoRequest request,
        CreateTemporaryTodoHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(new CreateTemporaryTodoCommand(request.Text, request.StartDate), cancellationToken);

        return result.IsSuccess
            ? TypedResults.Created((string?)null, result.MyValue)
            : result.ToProblem();
    }

    private static async Task<IResult> DeleteAsync( // DELETE /api/temporary-todos/{id}, возвращает 204 или 404, если временной задачи нет
        Guid id,
        DeleteTemporaryTodoHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(id, cancellationToken);

        return result.IsSuccess ? TypedResults.NoContent() : result.ToProblem();
    }

    private static async Task<NoContent> DeleteAllAsync( // DELETE /api/temporary-todos, удаляет все временные задачи и возвращает 204
        DeleteAllTemporaryTodosHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(cancellationToken);

        return TypedResults.NoContent();
    }
}
