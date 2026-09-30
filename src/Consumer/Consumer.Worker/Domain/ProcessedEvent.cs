namespace Consumer.Worker.Domain;

// Idempotency kaydı: bkz. docs/events.md -> İdempotency
public class ProcessedEvent
{
    public Guid EventId { get; set; }
    public DateTime ProcessedAtUtc { get; set; }
}
