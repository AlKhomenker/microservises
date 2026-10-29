using System.Diagnostics.CodeAnalysis;

namespace TodoApp.Shared.Kernel;

/// <summary>Outcome of an operation that either succeeds or fails with an <see cref="Kernel.Error"/>.</summary>
public interface IOperationResult
{
    /* --- Properties --- */
    Error? Error { get; } // причина ошибки, null при успехе
    [MemberNotNullWhen(false, nameof(Error))]
    bool IsSuccess { get; } // true, если ошибки нет
}

/// <summary>Outcome of an operation that either succeeds or fails with an <see cref="Kernel.Error"/>.</summary>
public class OperationResult : IOperationResult
{
    /* --- Properties --- */
    public Error? Error { get; } // причина ошибки, null при успехе; только get - задаётся только в конструкторе
    [MemberNotNullWhen(false, nameof(Error))]
    public bool IsSuccess => Error is null; // true, если ошибки нет

    /* --- Constructors --- */
    internal OperationResult(Error? error) => Error = error; // конструктор с ошибкой; internal - результат создаётся только через OperationResultFactory
}

public sealed class OperationResult<T> : OperationResult
{
    /* --- Properties --- */
    private readonly T? _myValue; // значение результата || null, при ошибке бросает InvalidOperationException
    public T MyValue => IsSuccess
        ? _myValue!
        : throw new InvalidOperationException("Cannot read the value of a failed result.");

    /* --- Constructors --- */
    internal OperationResult(T value) : base(null) => _myValue = value; // конструктор успешного результата со значением; internal - создаётся только через OperationResultFactory
    internal OperationResult(Error error) : base(error) { } // конструктор неуспешного результата с ошибкой; internal - создаётся только через OperationResultFactory

    /* --- Methods --- */
#pragma warning disable CA2225 // Неявные преобразования делают код обработчиков короче.
    public static implicit operator OperationResult<T>(T value) => OperationResultFactory.Success(value); // неявное преобразование: значение -> успешный OperationResult<T>, создание делегируется фабрике
    public static implicit operator OperationResult<T>(Error error) => OperationResultFactory.Failure<T>(error); // неявное преобразование: ошибка -> неуспешный OperationResult<T>, создание делегируется фабрике
#pragma warning restore CA2225
}
