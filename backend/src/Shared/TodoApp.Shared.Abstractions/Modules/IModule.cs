using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace TodoApp.Shared.Abstractions.Modules;

/// <summary>
/// Entry point of a module. The host knows modules only through this contract,
/// so each module keeps its internals private.
/// </summary>
public interface IModule
{
    /* --- Properties --- */
    string Name { get; } // имя модуля

    /* --- Methods --- */
    void RegisterServices(IServiceCollection services, IConfiguration configuration); // регистрирует сервисы модуля в DI
    void MapEndpoints(IEndpointRouteBuilder endpoints); // добавляет HTTP endpoints модуля
}
