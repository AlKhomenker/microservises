namespace TodoApp.Modules.TemporaryTodos.Application.CreateTemporaryTodo;

public sealed record CreateTemporaryTodoCommand(string? Text, DateOnly? StartDate); // primary-конструктор с текстом и датой начала новой временной задачи
