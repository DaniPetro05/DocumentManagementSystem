using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace OcrWorker;

public class OcrWorkerService : BackgroundService
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly ILogger<OcrWorkerService> _logger;
    private const string QueueName = "ocr-queue";

    public OcrWorkerService(ILogger<OcrWorkerService> logger, IConfiguration config)
    {
        _logger = logger;

        var factory = new ConnectionFactory
        {
            HostName = config["RABBITMQ_HOST"] ?? "rabbitmq",
            UserName = config["RABBITMQ_USER"] ?? "admin",
            Password = config["RABBITMQ_PASS"] ?? "secure_password",
            AutomaticRecoveryEnabled = true
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        _channel.QueueDeclare(QueueName, durable: true, exclusive: false, autoDelete: false);
        _channel.BasicQos(0, 1, false);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("OCR Worker started. Listening on '{Queue}'.", QueueName);

        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.Received += async (model, ea) =>
        {
            var body = Encoding.UTF8.GetString(ea.Body.ToArray());
            _logger.LogInformation("Message received: {Body}", body);

            try
            {
                var msg = JsonSerializer.Deserialize<DocumentMessage>(body);

                //SPRINT 4: OCR processing will be written here
                _logger.LogInformation(
                    "Processing DocId={Id} ('{Title}', {Format}) — OCR not yet implemented.",
                    msg.DocumentId, msg.Title, msg.Format);

                _channel.BasicAck(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process message {Tag}.", ea.DeliveryTag);
                _channel.BasicNack(ea.DeliveryTag, false, requeue: false);
            }
        };

        _channel.BasicConsume(QueueName, autoAck: false, consumer: consumer);

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
        await Task.Delay(Timeout.Infinite, cts.Token);
    }

    public override void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
        base.Dispose();
    }
}