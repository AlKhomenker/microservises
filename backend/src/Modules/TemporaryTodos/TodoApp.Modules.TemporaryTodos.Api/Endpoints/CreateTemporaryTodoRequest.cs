namespace TodoApp.Modules.TemporaryTodos.Api.Endpoints;

public sealed record CreateTemporaryTodoRequest(string? Text, DateOnly? StartDate); // primary-конструктор с текстом и датой начала новой временной задачи из тела запроса
