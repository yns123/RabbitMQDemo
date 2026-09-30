using Consumer.Worker.Domain;
using Consumer.Worker.Messaging;
using Consumer.Worker.Repositories;
using Consumer.Worker.Tests.TestSupport;
using Microsoft.Extensions.Logging.Abstractions;
using RabbitMqDemo.Contracts;

namespace Consumer.Worker.Tests.Messaging;

public class TaskCreatedEventHandlerTests : IDisposable
{
    private readonly SqliteInMemoryContextFactory _factory = new();

    [Fact]
    public async Task HandleAsync_NewEvent_SavesTaskAndMarksEventAsProcessed()
    {
        using var context = _factory.CreateContext();
        var handler = new TaskCreatedEventHandler(
            new TaskRepository(context),
            new ProcessedEventRepository(context),
            context,
            NullLogger<TaskCreatedEventHandler>.Instance);

        var taskCreatedEvent = new TaskCreatedEvent(
            EventId: Guid.NewGuid(),
            TaskId: Guid.NewGuid(),
            Title: "Yeni görev",
            Description: "açıklama",
            CreatedAtUtc: DateTime.UtcNow);

        await handler.HandleAsync(taskCreatedEvent, CancellationToken.None);

        Assert.NotNull(await context.Tasks.FindAsync(taskCreatedEvent.TaskId));
        Assert.NotNull(await context.ProcessedEvents.FindAsync(taskCreatedEvent.EventId));
    }

    [Fact]
    public async Task HandleAsync_DuplicateEvent_DoesNotInsertTaskAgain()
    {
        var taskCreatedEvent = new TaskCreatedEvent(
            EventId: Guid.NewGuid(),
            TaskId: Guid.NewGuid(),
            Title: "Yeni görev",
            Description: "açıklama",
            CreatedAtUtc: DateTime.UtcNow);

        // İlk işleme: event ilk kez gelir, Task ve ProcessedEvent kaydedilir.
        using (var context = _factory.CreateContext())
        {
            var handler = new TaskCreatedEventHandler(
                new TaskRepository(context),
                new ProcessedEventRepository(context),
                context,
                NullLogger<TaskCreatedEventHandler>.Instance);

            await handler.HandleAsync(taskCreatedEvent, CancellationToken.None);
        }

        // İkinci işleme: RabbitMQ aynı mesajı tekrar teslim eder (bkz. docs/events.md -> İdempotency).
        using (var context = _factory.CreateContext())
        {
            var handler = new TaskCreatedEventHandler(
                new TaskRepository(context),
                new ProcessedEventRepository(context),
                context,
                NullLogger<TaskCreatedEventHandler>.Instance);

            await handler.HandleAsync(taskCreatedEvent, CancellationToken.None);
        }

        using var verifyContext = _factory.CreateContext();
        var taskCount = verifyContext.Tasks.Count(t => t.Id == taskCreatedEvent.TaskId);

        Assert.Equal(1, taskCount);
    }

    public void Dispose() => _factory.Dispose();
}
