using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TodoApp.Shared.Kernel.Events;

namespace TodoApp.Shared.Abstractions.Events;

/// <summary>
/// Calls every registered consumer in the same process and request scope.
/// A failing consumer is logged and does not fail the publisher's operation: the change is already saved.
/// </summary>
internal sealed partial class InProcessEventPublisher( // primary-конструктор с provider текущего scope и логгером
    IServiceProvider serviceProvider,
    ILogger<InProcessEventPublisher> logger) : IEventPublisher
{
    /* --- Methods --- */
    public async Task PublishAsync<TEvent>(TEvent integrationEvent, CancellationToken cancellationToken) // по очереди вызывает всех получателей события; ошибку получателя пишет в лог
        where TEvent : IIntegrationEvent
    {
        foreach (var consumer in serviceProvider.GetServices<IIntegrationEventConsumer<TEvent>>())
        {
            try
            {
                await consumer.HandleAsync(integrationEvent, cancellationToken);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                LogConsumerFailed(logger, exception, consumer.GetType().Name, typeof(TEvent).Name);
            }
        }
    }
    [LoggerMessage(Level = LogLevel.Error, Message = "Consumer {Consumer} failed to handle {Event}.")]
    private static partial void LogConsumerFailed(ILogger logger, Exception exception, string consumer, string @event); // пишет в лог ошибку получателя; тело метода генерируется
}
