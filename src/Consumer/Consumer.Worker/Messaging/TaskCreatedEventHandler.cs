using Consumer.Worker.Data;
using Consumer.Worker.Domain;
using Consumer.Worker.Repositories;
using RabbitMqDemo.Contracts;

namespace Consumer.Worker.Messaging;

// Task kaydı + ProcessedEvent kaydı aynı SaveChangesAsync çağrısında, tek transaction'da yazılır.
// Bkz. .claude/skills/repository-pattern/SKILL.md
public class TaskCreatedEventHandler(
    ITaskRepository taskRepository,
    IProcessedEventRepository processedEventRepository,
    ConsumerDbContext dbContext,
    ILogger<TaskCreatedEventHandler> logger)
{
    public async Task HandleAsync(TaskCreatedEvent @event, CancellationToken cancellationToken)
    {
        if (await processedEventRepository.ExistsAsync(@event.EventId, cancellationToken))
        {
            logger.LogInformation(
                "EventId {EventId} zaten işlenmiş, tekrar işlenmiyor (idempotency).", @event.EventId);
            return;
        }

        taskRepository.Add(new TaskItem
        {
            Id = @event.TaskId,
            Title = @event.Title,
            Description = @event.Description,
            CreatedAtUtc = @event.CreatedAtUtc
        });

        processedEventRepository.Add(new ProcessedEvent
        {
            EventId = @event.EventId,
            ProcessedAtUtc = DateTime.UtcNow
        });

        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Task kaydedildi. TaskId={TaskId} Title={Title} EventId={EventId}",
            @event.TaskId, @event.Title, @event.EventId);
    }
}
