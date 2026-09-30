using Consumer.Worker.Domain;

namespace Consumer.Worker.Repositories;

public interface IProcessedEventRepository
{
    Task<bool> ExistsAsync(Guid eventId, CancellationToken cancellationToken = default);
    void Add(ProcessedEvent processedEvent);
}
