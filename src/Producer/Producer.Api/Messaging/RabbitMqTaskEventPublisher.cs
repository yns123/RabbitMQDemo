using System.Text.Json;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMqDemo.Contracts;

namespace Producer.Api.Messaging;

public class RabbitMqTaskEventPublisher : ITaskEventPublisher, IAsyncDisposable
{
    // docs/events.md ile aynı: exchange, queue ve routing key burada tanımlı.
    private const string ExchangeName = "rabbitmqdemo";
    private const string QueueName = "task-created-queue";
    private const string RoutingKey = "task.created";

    private readonly RabbitMqOptions _options;
    private readonly ILogger<RabbitMqTaskEventPublisher> _logger;
    private readonly SemaphoreSlim _connectionLock = new(1, 1);
    private IConnection? _connection;

    public RabbitMqTaskEventPublisher(IOptions<RabbitMqOptions> options, ILogger<RabbitMqTaskEventPublisher> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task PublishTaskCreatedAsync(TaskCreatedEvent @event, CancellationToken cancellationToken = default)
    {
        var connection = await GetConnectionAsync(cancellationToken);
        await using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

        await channel.ExchangeDeclareAsync(ExchangeName, ExchangeType.Direct, durable: true, cancellationToken: cancellationToken);
        await channel.QueueDeclareAsync(QueueName, durable: true, exclusive: false, autoDelete: false, cancellationToken: cancellationToken);
        await channel.QueueBindAsync(QueueName, ExchangeName, RoutingKey, cancellationToken: cancellationToken);

        var body = JsonSerializer.SerializeToUtf8Bytes(@event);
        var properties = new BasicProperties
        {
            Persistent = true,
            ContentType = "application/json"
        };

        await channel.BasicPublishAsync(
            exchange: ExchangeName,
            routingKey: RoutingKey,
            mandatory: false,
            basicProperties: properties,
            body: body,
            cancellationToken: cancellationToken);

        _logger.LogInformation(
            "TaskCreatedEvent yayınlandı. TaskId={TaskId} EventId={EventId} RoutingKey={RoutingKey}",
            @event.TaskId, @event.EventId, RoutingKey);
    }

    private async Task<IConnection> GetConnectionAsync(CancellationToken cancellationToken)
    {
        if (_connection is { IsOpen: true })
        {
            return _connection;
        }

        await _connectionLock.WaitAsync(cancellationToken);
        try
        {
            if (_connection is { IsOpen: true })
            {
                return _connection;
            }

            var factory = new ConnectionFactory
            {
                HostName = _options.HostName,
                Port = _options.Port,
                UserName = _options.UserName,
                Password = _options.Password
            };

            _connection = await factory.CreateConnectionAsync(cancellationToken);
            return _connection;
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection is not null)
        {
            await _connection.DisposeAsync();
        }
    }
}
