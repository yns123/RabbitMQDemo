using Microsoft.AspNetCore.Mvc;
using Producer.Api.Messaging;
using Producer.Api.Models;
using RabbitMqDemo.Contracts;

namespace Producer.Api.Controllers;

[ApiController]
[Route("api/tasks")]
public class TasksController : ControllerBase
{
    private readonly ITaskEventPublisher _publisher;
    private readonly ILogger<TasksController> _logger;

    public TasksController(ITaskEventPublisher publisher, ILogger<TasksController> logger)
    {
        _publisher = publisher;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTaskRequest request, CancellationToken cancellationToken)
    {
        var taskCreatedEvent = new TaskCreatedEvent(
            EventId: Guid.NewGuid(),
            TaskId: Guid.NewGuid(),
            Title: request.Title,
            Description: request.Description,
            CreatedAtUtc: DateTime.UtcNow);

        try
        {
            await _publisher.PublishTaskCreatedAsync(taskCreatedEvent, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "TaskCreatedEvent yayınlanamadı. TaskId={TaskId}", taskCreatedEvent.TaskId);
            return Problem(
                title: "Event yayınlanamadı",
                detail: "RabbitMQ'ya bağlanılamadı veya mesaj yayınlanamadı.",
                statusCode: StatusCodes.Status500InternalServerError);
        }

        return Accepted(new { taskId = taskCreatedEvent.TaskId, eventId = taskCreatedEvent.EventId });
    }
}
