using TodoApp.Shared.Kernel;

namespace TodoApp.Modules.Todos.Domain;

public sealed class TodoItem
{
    /* --- Properties --- */
    public const int TextMaxLength = 500; // максимальная длина текста
    public Guid Id { get; private set; } // id задачи; private set - нельзя изменить снаружи
    public string Text { get; private set; } = string.Empty; // текст задачи; private set - нельзя изменить снаружи
    public DateTimeOffset CreateDate { get; private set; } // дата создания задачи; private set - нельзя изменить снаружи

    /* --- Constructors --- */
    private TodoItem() { } // пустой конструктор, нужен для EF Core
    private TodoItem(Guid id, string text, DateTimeOffset createDate) // конструктор с id, текстом и датой создания
    {
        Id = id;
        Text = text;
        CreateDate = createDate;
    }

    /* --- Methods --- */
    public static OperationResult<TodoItem> Create(string? text, DateTimeOffset createDate) // фабричный метод Create, возвращает OperationResult<TodoItem>: успех с объектом TodoItem или ошибку
    {
        var normalized = text?.Trim();

        if (string.IsNullOrEmpty(normalized))
        {
            return TodoErrors.TextRequired;
        }

        if (normalized.Length > TextMaxLength)
        {
            return TodoErrors.TextTooLong;
        }

        return new TodoItem(Guid.CreateVersion7(createDate), normalized, createDate);
    }
}
