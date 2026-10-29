using Microsoft.Extensions.DependencyInjection;
using TodoApp.Modules.Todos.Application.CreateTodo;
using TodoApp.Modules.Todos.Application.DeleteAllTodos;
using TodoApp.Modules.Todos.Application.DeleteTodo;
using TodoApp.Modules.Todos.Application.GetTodos;

namespace TodoApp.Modules.Todos.Application;

public static class DependencyInjection
{
    /* --- Methods --- */
    public static IServiceCollection AddTodosApplication(this IServiceCollection services) // регистрирует в DI все обработчики Application; возвращает services для цепочки вызовов
    {
        services.AddScoped<GetTodosHandler>();
        services.AddScoped<CreateTodoHandler>();
        services.AddScoped<DeleteTodoHandler>();
        services.AddScoped<DeleteAllTodosHandler>();

        return services;
    }
}
