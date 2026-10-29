namespace TodoApp.Modules.Todos.Api.Endpoints;

public sealed record CreateTodoRequest(string? Text); // primary-конструктор с текстом новой задачи из тела запроса
