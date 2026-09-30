using RabbitMqDemo.Contracts;

namespace Producer.Api.Messaging;

public interface ITaskEventPublisher
{
    Task PublishTaskCreatedAsync(TaskCreatedEvent @event, CancellationToken cancellationToken = default);
}
