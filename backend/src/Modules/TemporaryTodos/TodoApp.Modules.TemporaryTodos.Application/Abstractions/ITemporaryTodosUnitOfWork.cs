using TodoApp.Shared.Kernel;

namespace TodoApp.Modules.TemporaryTodos.Application.Abstractions;

/// <summary>Unit of work scoped to the TemporaryTodos module's own database context.</summary>
public interface ITemporaryTodosUnitOfWork : IUnitOfWork;
