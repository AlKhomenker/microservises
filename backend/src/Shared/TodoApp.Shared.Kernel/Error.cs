namespace TodoApp.Shared.Kernel;

public enum ErrorType
{
    Validation, // входные данные неверны
    NotFound, // объект не найден
}

/// <summary>Describes an expected failure of a business operation.</summary>
public sealed record Error(string Code, string Message, ErrorType Type) // primary-конструктор с кодом ошибки, текстом сообщения и типом ошибки
{
    /* --- Methods --- */
    public static Error Validation(string code, string message) => new(code, message, ErrorType.Validation); // фабричный метод Validation, возвращает Error с типом Validation
    public static Error NotFound(string code, string message) => new(code, message, ErrorType.NotFound); // фабричный метод NotFound, возвращает Error с типом NotFound
}
