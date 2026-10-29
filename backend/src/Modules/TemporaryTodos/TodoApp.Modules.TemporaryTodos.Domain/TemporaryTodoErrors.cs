using TodoApp.Shared.Kernel;

namespace TodoApp.Modules.TemporaryTodos.Domain;

public static class TemporaryTodoErrors
{
    /* --- Properties --- */
    public static readonly Error TextRequired = // ошибка валидации: текст пустой
        Error.Validation("text", "Text is required.");
    public static readonly Error TextTooLong = // ошибка валидации: текст длиннее TemporaryTodoItem.TextMaxLength
        Error.Validation("text", $"Text must be at most {TemporaryTodoItem.TextMaxLength} characters.");
    public static readonly Error StartDateRequired = // ошибка валидации: дата начала не указана
        Error.Validation("startDate", "Start date is required.");

    /* --- Methods --- */
    public static Error NotFound(Guid id) => // возвращает ошибку «не найдено» для временной задачи с этим id
        Error.NotFound("temporary_todos.not_found", $"Temporary todo item '{id}' was not found.");
}
