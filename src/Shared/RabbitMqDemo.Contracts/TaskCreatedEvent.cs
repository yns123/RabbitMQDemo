namespace RabbitMqDemo.Contracts;

public record TaskCreatedEvent(
    Guid EventId,
    Guid TaskId,
    string Title,
    string? Description,
    DateTime CreatedAtUtc);
