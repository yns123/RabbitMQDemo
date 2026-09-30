using Consumer.Worker.Data;
using Consumer.Worker.Domain;
using Microsoft.EntityFrameworkCore;

namespace Consumer.Worker.Repositories;

public class ProcessedEventRepository(ConsumerDbContext context) : IProcessedEventRepository
{
    public Task<bool> ExistsAsync(Guid eventId, CancellationToken cancellationToken = default) =>
        context.ProcessedEvents.AnyAsync(e => e.EventId == eventId, cancellationToken);

    public void Add(ProcessedEvent processedEvent) => context.ProcessedEvents.Add(processedEvent);
}
