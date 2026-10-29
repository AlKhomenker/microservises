namespace TodoApp.Shared.Kernel.Events;

public interface IIntegrationEventConsumer<in TEvent>
    where TEvent : IIntegrationEvent
{
    /* --- Methods --- */
    Task HandleAsync(TEvent integrationEvent, CancellationToken cancellationToken); // обрабатывает событие
}
