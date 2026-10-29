using Microsoft.Extensions.DependencyInjection;
using TodoApp.Shared.Kernel.Events;

namespace TodoApp.Shared.Abstractions.Events;

public static class DependencyInjection
{
    /* --- Methods --- */
    public static IServiceCollection AddInProcessEvents(this IServiceCollection services) // регистрирует в DI публикатор событий внутри процесса; возвращает services для цепочки вызовов
    {
        services.AddScoped<IEventPublisher, InProcessEventPublisher>();

        return services;
    }
}
