using Consumer.Worker.Domain;
using Consumer.Worker.Repositories;
using Consumer.Worker.Tests.TestSupport;

namespace Consumer.Worker.Tests.Repositories;

public class TaskRepositoryTests : IDisposable
{
    private readonly SqliteInMemoryContextFactory _factory = new();

    [Fact]
    public async Task Add_PersistsTask_AfterSaveChanges()
    {
        var taskId = Guid.NewGuid();

        using (var context = _factory.CreateContext())
        {
            var repository = new TaskRepository(context);
            repository.Add(new TaskItem
            {
                Id = taskId,
                Title = "Test görevi",
                Description = "açıklama",
                CreatedAtUtc = DateTime.UtcNow
            });

            await context.SaveChangesAsync();
        }

        using var verifyContext = _factory.CreateContext();
        var saved = await verifyContext.Tasks.FindAsync(taskId);

        Assert.NotNull(saved);
        Assert.Equal("Test görevi", saved!.Title);
    }

    public void Dispose() => _factory.Dispose();
}
