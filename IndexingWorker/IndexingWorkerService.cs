using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using Elastic.Clients.Elasticsearch;
using Elastic.Transport;

namespace IndexingWorker;

public class IndexingWorkerService : BackgroundService
{
    private IConnection? _connection;
    private IChannel? _channel;
    private readonly ILogger<IndexingWorkerService> _logger;
    private readonly IConfiguration _config;
    private readonly ElasticsearchClient _esClient;
    private const string QueueName = "index-queue";
    private const string IndexName = "documents";

    public IndexingWorkerService(ILogger<IndexingWorkerService> logger, IConfiguration config)
    {
        _logger = logger;
        _config = config;

        var esUrl = _config["ELASTICSEARCH_URL"] ?? "http://elasticsearch:9200";
        var esUser = _config["ELASTICSEARCH_USER"];
        var esPass = _config["ELASTICSEARCH_PASS"];

        var settings = new ElasticsearchClientSettings(new Uri(esUrl))
            .RequestTimeout(TimeSpan.FromSeconds(30));

        if (!string.IsNullOrEmpty(esUser) && !string.IsNullOrEmpty(esPass))
        {
            settings = settings.Authentication(new BasicAuthentication(esUser, esPass));
        }

        _esClient = new ElasticsearchClient(settings);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _config["RABBITMQ_HOST"] ?? "rabbitmq",
            UserName = _config["RABBITMQ_USER"] ?? "admin",
            Password = _config["RABBITMQ_PASS"] ?? "secure_password",
            AutomaticRecoveryEnabled = true
        };

        _connection = await factory.CreateConnectionAsync();
        _channel = await _connection.CreateChannelAsync();

        await _channel.QueueDeclareAsync(QueueName, durable: true, exclusive: false, autoDelete: false);
        await _channel.BasicQosAsync(0, 1, false);

        _logger.LogInformation("Indexing Worker started. Listening on '{Queue}'.", QueueName);

        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.ReceivedAsync += async (model, ea) =>
        {
            var body = Encoding.UTF8.GetString(ea.Body.Span);
            var msg = JsonSerializer.Deserialize<OcrResultMessage>(body);

            if (msg is null)
            {
                _logger.LogWarning("Failed to deserialize OCR result, skipping.");
                await _channel!.BasicAckAsync(ea.DeliveryTag, false);
                return;
            }

            try
            {
                _logger.LogInformation("Indexing DocId={Id} ('{Title}') into Elasticsearch.",
                    msg.DocumentId, msg.Title);

                var doc = new DocumentIndex
                {
                    Id = msg.DocumentId,
                    Title = msg.Title,
                    Text = msg.TextContent,
                    Confidence = msg.MeanConfidence,
                    ProcessedAt = msg.ProcessedAt
                };

                var response = await _esClient.IndexAsync(doc,
                    i => i.Index(IndexName).Id(doc.Id.ToString()));

                if (response.IsValidResponse)
                {
                    _logger.LogInformation("Indexed DocId={Id} successfully.", msg.DocumentId);
                }
                else
                {
                    _logger.LogError("Elasticsearch indexing failed: {Error}", response.DebugInformation);
                }

                await _channel!.BasicAckAsync(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to index message {Tag}.", ea.DeliveryTag);
                await _channel!.BasicNackAsync(ea.DeliveryTag, false, requeue: false);
            }
        };

        await _channel.BasicConsumeAsync(QueueName, autoAck: false, consumer: consumer);

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

public class DocumentIndex
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Text { get; set; } = "";
    public double Confidence { get; set; }
    public DateTime ProcessedAt { get; set; }
}