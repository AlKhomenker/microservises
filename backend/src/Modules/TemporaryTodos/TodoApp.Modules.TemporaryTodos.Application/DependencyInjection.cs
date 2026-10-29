using Microsoft.Extensions.DependencyInjection;
using TodoApp.Modules.TemporaryTodos.Application.CreateTemporaryTodo;
using TodoApp.Modules.TemporaryTodos.Application.DeleteAllTemporaryTodos;
using TodoApp.Modules.TemporaryTodos.Application.DeleteTemporaryTodo;
using TodoApp.Modules.TemporaryTodos.Application.GetTemporaryTodos;

namespace TodoApp.Modules.TemporaryTodos.Application;

public static class DependencyInjection
{
    /* --- Methods --- */
    public static IServiceCollection AddTemporaryTodosApplication(this IServiceCollection services) // регистрирует в DI все обработчики Application; возвращает services для цепочки вызовов
    {
        services.AddScoped<GetTemporaryTodosHandler>();
        services.AddScoped<CreateTemporaryTodoHandler>();
        services.AddScoped<DeleteTemporaryTodoHandler>();
        services.AddScoped<DeleteAllTemporaryTodosHandler>();

        return services;
    }
}
