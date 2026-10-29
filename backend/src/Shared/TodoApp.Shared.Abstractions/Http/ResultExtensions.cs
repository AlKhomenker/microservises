using Microsoft.AspNetCore.Http;
using TodoApp.Shared.Kernel;

namespace TodoApp.Shared.Abstractions.Http;

public static class ResultExtensions
{
    /* --- Methods --- */
    /// <summary>Maps a failed <see cref="OperationResult"/> to an RFC 9457 problem response.</summary>
    public static IResult ToProblem(this OperationResult result) // превращает неуспешный OperationResult в ответ 400 или 404 (problem); для успешного OperationResult бросает исключение
    {
        ArgumentNullException.ThrowIfNull(result);

        if (result.IsSuccess)
        {
            throw new InvalidOperationException("Cannot create a problem response from a successful result.");
        }

        var error = result.Error;

        return error.Type switch
        {
            ErrorType.Validation => TypedResults.ValidationProblem(
                new Dictionary<string, string[]> { [error.Code] = [error.Message] },
                title: "One or more validation errors occurred."),
            ErrorType.NotFound => TypedResults.Problem(
                title: "Resource not found.",
                detail: error.Message,
                statusCode: StatusCodes.Status404NotFound,
                extensions: new Dictionary<string, object?> { ["code"] = error.Code }),
            _ => TypedResults.Problem(statusCode: StatusCodes.Status500InternalServerError),
        };
    }
}
