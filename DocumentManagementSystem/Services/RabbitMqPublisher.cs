using RabbitMQ.AMQP.Client;
using RabbitMQ.AMQP.Client.Impl;
using System.Text;
using System.Text.Json;
using DocumentManagementSystem.Exceptions;
using DocumentManagementSystem.Models;

namespace DocumentManagementSystem.Services;

public class RabbitMqPublisher : IDisposable
{
    private readonly IEnvironment _environment;
    private readonly IConnection _connection;
    private readonly IPublisher _publisher;
    private readonly ILogger<RabbitMqPublisher> _logger;
    private const string QueueName = "ocr-queue";

    public RabbitMqPublisher(IConfiguration config, ILogger<RabbitMqPublisher> logger)
    {
        _logger = logger;

        var brokerUri = $"amqp://{config["RABBITMQ_USER"]}:{config["RABBITMQ_PASS"]}" +
                        $"@{config["RABBITMQ_HOST"]}:5672/%2f";

        try
        {
            var settings = ConnectionSettingsBuilder.Create()
                .Uri(new Uri(brokerUri))
                .ContainerId("document-api")
                .Build();

            _environment = AmqpEnvironment.Create(settings);
            _connection = _environment.CreateConnectionAsync().GetAwaiter().GetResult();

            var management = _connection.Management();
            var queueSpec = management.Queue(QueueName).Type(QueueType.QUORUM);
            queueSpec.DeclareAsync().GetAwaiter().GetResult();

            _publisher = _connection.PublisherBuilder()
                .Queue(QueueName)
                .BuildAsync().GetAwaiter().GetResult();

            _logger.LogInformation("RabbitMQ connected. Queue '{Queue}' ready.", QueueName);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Failed to connect to RabbitMQ");
            throw new MessagingException("Cannot connect to RabbitMQ", ex);
        }
    }

    public async Task PublishDocumentAsync(DocumentMessage message)
    {
        try
        {
            var json = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(json);
            var amqpMessage = new AmqpMessage(body);

            var result = await _publisher.PublishAsync(amqpMessage);

            if (result.Outcome.State != OutcomeState.Accepted)
                throw new MessagingException($"Message rejected: {result.Outcome.State}");

            _logger.LogInformation("Published to '{Queue}': DocId={Id}, Title='{Title}'",
                QueueName, message.DocumentId, message.Title);
        }
        catch (MessagingException) { throw; }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Publish failed for DocId={Id}", message.DocumentId);
            throw new MessagingException($"Publish failed for document {message.DocumentId}", ex);
        }
    }

    public void Dispose()
    {
        _publisher?.CloseAsync().GetAwaiter().GetResult();
        _connection?.Dispose();
        _environment?.CloseAsync().GetAwaiter().GetResult();
    }
}