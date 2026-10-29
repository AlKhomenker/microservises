using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TodoApp.Modules.Todos.Api.Endpoints;
using TodoApp.Modules.Todos.Application;
using TodoApp.Modules.Todos.Infrastructure;
using TodoApp.Shared.Abstractions.Modules;

namespace TodoApp.Modules.Todos.Api;

public sealed class TodosModule : IModule
{
    /* --- Properties --- */
    public string Name => "Todos"; // имя модуля

    /* --- Methods --- */
    public void RegisterServices(IServiceCollection services, IConfiguration configuration) // регистрирует в DI сервисы Application и Infrastructure
    {
        services.AddTodosApplication();
        services.AddTodosInfrastructure(configuration);
    }
    public void MapEndpoints(IEndpointRouteBuilder endpoints) => endpoints.MapTodoEndpoints(); // добавляет HTTP endpoints задач
}
