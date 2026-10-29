using TodoApp.Shared.Kernel;

namespace TodoApp.Modules.TemporaryTodos.Domain;

public sealed class TemporaryTodoItem
{
    /* --- Properties --- */
    public const int TextMaxLength = 500; // максимальная длина текста
    public Guid Id { get; private set; } // id задачи; private set - нельзя изменить снаружи
    public string Text { get; private set; } = string.Empty; // текст задачи; private set - нельзя изменить снаружи
    public DateOnly StartDate { get; private set; } // день, на который задача; private set - нельзя изменить снаружи

    /* --- Constructors --- */
    private TemporaryTodoItem() { } // пустой конструктор, нужен для EF Core
    private TemporaryTodoItem(Guid id, string text, DateOnly startDate) // конструктор с id, текстом и датой начала
    {
        Id = id;
        Text = text;
        StartDate = startDate;
    }

    /* --- Methods --- */
    public static OperationResult<TemporaryTodoItem> Create(string? text, DateOnly? startDate, DateTimeOffset createdAt) // фабричный метод Create, возвращает OperationResult<TemporaryTodoItem>: успех с объектом или ошибку
    {
        var normalized = text?.Trim();

        if (string.IsNullOrEmpty(normalized))
        {
            return TemporaryTodoErrors.TextRequired;
        }

        if (normalized.Length > TextMaxLength)
        {
            return TemporaryTodoErrors.TextTooLong;
        }

        if (startDate is null)
        {
            return TemporaryTodoErrors.StartDateRequired;
        }

        return new TemporaryTodoItem(Guid.CreateVersion7(createdAt), normalized, startDate.Value);
    }

    /// <summary>A temporary todo is active on its start date and expires once that day has passed.</summary>
    public TemporaryTodoStatus GetStatus(DateOnly today) => // возвращает Expired, если дата начала уже прошла, иначе Active
        StartDate < today ? TemporaryTodoStatus.Expired : TemporaryTodoStatus.Active;
}
