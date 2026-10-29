using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TodoApp.Modules.TemporaryTodos.Api.Endpoints;
using TodoApp.Modules.TemporaryTodos.Application;
using TodoApp.Modules.TemporaryTodos.Infrastructure;
using TodoApp.Shared.Abstractions.Modules;

namespace TodoApp.Modules.TemporaryTodos.Api;

public sealed class TemporaryTodosModule : IModule
{
    /* --- Properties --- */
    public string Name => "TemporaryTodos"; // имя модуля

    /* --- Methods --- */
    public void RegisterServices(IServiceCollection services, IConfiguration configuration) // регистрирует в DI сервисы Application и Infrastructure
    {
        services.AddTemporaryTodosApplication();
        services.AddTemporaryTodosInfrastructure(configuration);
    }
    public void MapEndpoints(IEndpointRouteBuilder endpoints) => endpoints.MapTemporaryTodoEndpoints(); // добавляет HTTP endpoints временных задач
}
