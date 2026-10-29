using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using TodoApp.Modules.Todos.Application.Contracts;
using TodoApp.Modules.Todos.Application.CreateTodo;
using TodoApp.Modules.Todos.Application.DeleteAllTodos;
using TodoApp.Modules.Todos.Application.DeleteTodo;
using TodoApp.Modules.Todos.Application.GetTodos;
using TodoApp.Shared.Abstractions.Http;

namespace TodoApp.Modules.Todos.Api.Endpoints;

internal static class TodoEndpoints
{
    /* --- Properties --- */
    public const string BasePath = "/api/todos"; // базовый маршрут endpoints задач

    /* --- Methods --- */
    public static IEndpointRouteBuilder MapTodoEndpoints(this IEndpointRouteBuilder endpoints) // добавляет маршруты GET, POST и DELETE для задач; возвращает endpoints для цепочки вызовов
    {
        var group = endpoints.MapGroup(BasePath).WithTags("Todos");

        group.MapGet("/", GetAllAsync)
            .WithName("GetTodos")
            .WithSummary("Returns all todo items, newest first.");

        group.MapPost("/", CreateAsync)
            .WithName("CreateTodo")
            .WithSummary("Creates a todo item.")
            .Produces<TodoItemDto>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        group.MapDelete("/{id:guid}", DeleteAsync)
            .WithName("DeleteTodo")
            .WithSummary("Deletes a todo item.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/", DeleteAllAsync)
            .WithName("DeleteAllTodos")
            .WithSummary("Deletes all todo items.")
            .Produces(StatusCodes.Status204NoContent);

        return endpoints;
    }

    private static async Task<Ok<IReadOnlyList<TodoItemDto>>> GetAllAsync( // GET /api/todos, возвращает 200 со всеми задачами
        GetTodosHandler handler,
        CancellationToken cancellationToken) =>
        TypedResults.Ok(await handler.HandleAsync(cancellationToken));

    private static async Task<IResult> CreateAsync( // POST /api/todos, возвращает 201 с новой задачей или 400 при ошибке валидации
        CreateTodoRequest request,
        CreateTodoHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(new CreateTodoCommand(request.Text), cancellationToken);

        return result.IsSuccess
            ? TypedResults.Created((string?)null, result.MyValue)
            : result.ToProblem();
    }

    private static async Task<IResult> DeleteAsync( // DELETE /api/todos/{id}, возвращает 204 или 404, если задачи нет
        Guid id,
        DeleteTodoHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(id, cancellationToken);

        return result.IsSuccess ? TypedResults.NoContent() : result.ToProblem();
    }

    private static async Task<NoContent> DeleteAllAsync( // DELETE /api/todos, удаляет все задачи и возвращает 204
        DeleteAllTodosHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(cancellationToken);

        return TypedResults.NoContent();
    }
}
