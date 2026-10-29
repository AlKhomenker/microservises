namespace TodoApp.Modules.Todos.Application.CreateTodo;

public sealed record CreateTodoCommand(string? Text); // primary-конструктор с текстом новой задачи
