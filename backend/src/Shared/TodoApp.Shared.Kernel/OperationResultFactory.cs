namespace TodoApp.Shared.Kernel;

/// <summary>Creates <see cref="OperationResult"/> and <see cref="OperationResult{T}"/> instances; the only place where results are constructed.</summary>
public static class OperationResultFactory
{
    /* --- Methods --- */
    public static OperationResult Success() => new(null); // фабричный метод Success, возвращает успешный OperationResult
    public static OperationResult Failure(Error error) => new(error); // фабричный метод Failure, возвращает неуспешный OperationResult с переданной ошибкой

    public static OperationResult<T> Success<T>(T value) => new(value); // фабричный метод Success, возвращает успешный OperationResult<T> со значением
    public static OperationResult<T> Failure<T>(Error error) => new(error); // фабричный метод Failure, возвращает неуспешный OperationResult<T> с переданной ошибкой
}
