using TodoApp.Shared.Kernel;

namespace TodoApp.Modules.Todos.Domain;

public static class TodoErrors
{
    /* --- Properties --- */
    public static readonly Error TextRequired = // ошибка валидации: текст пустой
        Error.Validation("text", "Text is required.");
    public static readonly Error TextTooLong = // ошибка валидации: текст длиннее TodoItem.TextMaxLength
        Error.Validation("text", $"Text must be at most {TodoItem.TextMaxLength} characters.");

    /* --- Methods --- */
    public static Error NotFound(Guid id) => // возвращает ошибку «не найдено» для задачи с этим id
        Error.NotFound("todos.not_found", $"Todo item '{id}' was not found.");
}
