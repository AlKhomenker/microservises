namespace TodoApp.Modules.TemporaryTodos.Domain;

public enum TemporaryTodoStatus
{
    Active, // дата начала сегодня или позже
    Expired, // дата начала уже прошла
}
