using TodoApp.Shared.Kernel.Events;

namespace TodoApp.Modules.Todos.UnitTests.Fakes;

/// <summary>Test double that records published events.</summary>
internal sealed class FakeEventPublisher : IEventPublisher
{
    public List<IIntegrationEvent> Published { get; } = [];

    public Task PublishAsync<TEvent>(TEvent integrationEvent, CancellationToken cancellationToken)
        where TEvent : IIntegrationEvent
    {
        Published.Add(integrationEvent);
        return Task.CompletedTask;
    }
}
