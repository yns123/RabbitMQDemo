using Consumer.Worker.Domain;
using Consumer.Worker.Repositories;
using Consumer.Worker.Tests.TestSupport;

namespace Consumer.Worker.Tests.Repositories;

public class ProcessedEventRepositoryTests : IDisposable
{
    private readonly SqliteInMemoryContextFactory _factory = new();

    [Fact]
    public async Task ExistsAsync_ReturnsFalse_WhenEventNotProcessed()
    {
        using var context = _factory.CreateContext();
        var repository = new ProcessedEventRepository(context);

        var exists = await repository.ExistsAsync(Guid.NewGuid());

        Assert.False(exists);
    }

    [Fact]
    public async Task ExistsAsync_ReturnsTrue_WhenEventAlreadyProcessed()
    {
        var eventId = Guid.NewGuid();

        using (var context = _factory.CreateContext())
        {
            var repository = new ProcessedEventRepository(context);
            repository.Add(new ProcessedEvent { EventId = eventId, ProcessedAtUtc = DateTime.UtcNow });
            await context.SaveChangesAsync();
        }

        using var verifyContext = _factory.CreateContext();
        var verifyRepository = new ProcessedEventRepository(verifyContext);

        var exists = await verifyRepository.ExistsAsync(eventId);

        Assert.True(exists);
    }

    public void Dispose() => _factory.Dispose();
}
