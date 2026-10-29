using TodoApp.Shared.Kernel;

namespace TodoApp.Modules.Todos.Application.Abstractions;

/// <summary>Unit of work scoped to the Todos module's own database context.</summary>
public interface ITodosUnitOfWork : IUnitOfWork;
