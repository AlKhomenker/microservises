namespace TodoApp.Shared.Kernel.Events;

/// <summary>Sends integration events to every module that handles them. The publisher does not know the consumers.</summary>
public interface IEventPublisher
{
    /* --- Methods --- */
    Task PublishAsync<TEvent>(TEvent integrationEvent, CancellationToken cancellationToken) // передаёт событие всем его получателям
        where TEvent : IIntegrationEvent;
}
