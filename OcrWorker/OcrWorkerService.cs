using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using Shared;
using OcrWorker.Services;
using Minio;
using Minio.DataModel.Args;
using Tesseract;

namespace OcrWorker;

public class OcrWorkerService : BackgroundService
{
    private IConnection? _connection;
    private IChannel? _channel;
    private readonly ILogger<OcrWorkerService> _logger;
    private readonly IConfiguration _config;
    private readonly IMinioClient _minioClient;
    private readonly TesseractEngine _tesseract;
    private readonly PdfToImageConverter _pdfConverter;
    private const string QueueName = "ocr-queue";
    private const string IndexQueueName = "index-queue";
    private const string BucketName = "documents";

    public OcrWorkerService(ILogger<OcrWorkerService> logger, IConfiguration config)
    {
        _logger = logger;
        _config = config;

        _minioClient = new MinioClient()
            .WithEndpoint(config["MINIO_ENDPOINT"] ?? "minio:9000")
            .WithCredentials(
                config["MINIO_ACCESS_KEY"] ?? "minioadmin",
                config["MINIO_SECRET_KEY"] ?? "minioadmin")
            .WithSSL(false)
            .Build();

        //Tesseract: Zeige Richtung "tessdata" Ordner (language files)
        var tessDataPath = config["TESSDATA_PREFIX"] ?? "/usr/share/tesseract-ocr/5/tessdata";
        _tesseract = new TesseractEngine(tessDataPath, "eng", EngineMode.LstmOnly);

        _pdfConverter = new PdfToImageConverter();
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

        var quorumArgs = new Dictionary<string, object?> { ["x-queue-type"] = "quorum" };

        await _channel.QueueDeclareAsync(QueueName, durable: true, exclusive: false, autoDelete: false, arguments: quorumArgs);
        await _channel.QueueDeclareAsync(IndexQueueName, durable: true, exclusive: false, autoDelete: false, arguments: quorumArgs);
        await _channel.BasicQosAsync(0, 1, false);

        _logger.LogInformation("OCR Worker started. Listening on '{Queue}'.", QueueName);

        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.ReceivedAsync += async (model, ea) =>
        {
            var body = Encoding.UTF8.GetString(ea.Body.Span);
            _logger.LogInformation("Message received: {Body}", body);

            var msg = JsonSerializer.Deserialize<DocumentMessage>(body);

            if (msg is null)
            {
                _logger.LogWarning("Failed to deserialize message, skipping.");
                await _channel!.BasicAckAsync(ea.DeliveryTag, false);
                return;
            }

            try
            {
                //SPRINT 4: OCR processing will be written here
                /*_logger.LogInformation(
                    "Processing DocId={Id} ('{Title}', {Format}) — OCR not yet implemented.",
                    msg.DocumentId, msg.Title, msg.Format);

                await _channel!.BasicAckAsync(ea.DeliveryTag, false);*/

                /*_logger.LogInformation(
                    "Processing DocId={Id} ('{Title}') — Action: {Action}",
                    msg.DocumentId, msg.Title, msg.Action);

                switch (msg.Action)
                {
                    case DocumentAction.Created:
                        _logger.LogInformation("New document {Id} queued for OCR.", msg.DocumentId);
                        break;

                    case DocumentAction.Updated:
                        _logger.LogInformation("Document {Id} updated — will re-process if needed.", msg.DocumentId);
                        break;

                    case DocumentAction.Deleted:
                        _logger.LogInformation("Document {Id} deleted — will clean up resources.", msg.DocumentId);
                        break;
                }*/

                _logger.LogInformation(
                    "Processing DocId={Id} ('{Title}') — fetching from MinIO.",
                    msg.DocumentId, msg.Title);

                //1. Downloade PDF von MinIO
                var pdfPath = await DownloadFromMinio(msg.MinioObjectPath);

                //2. Konvertiere PDF Seiten zu Bilder
                var workDir = Path.Combine(Path.GetTempPath(), $"ocr-{msg.DocumentId}");
                var images = _pdfConverter.ConvertPdfToImages(pdfPath, workDir);
                _logger.LogInformation("Converted {Count} page(s) to images.", images.Count);

                //3. Nutze Tesseract auf jeder Seite
                var sb = new StringBuilder();
                double totalConfidence = 0;
                foreach (var imgPath in images)
                {
                    using var pix = Pix.LoadFromFile(imgPath);
                    using var page = _tesseract.Process(pix);
                    sb.Append(page.GetText()).Append('\n');
                    totalConfidence += page.GetMeanConfidence();
                }
                double meanConfidence = images.Count > 0 ? totalConfidence / images.Count : 0;

                //4. Publishe Ergebnis zu index-queue
                var ocrResult = new OcrResultMessage
                {
                    DocumentId = msg.DocumentId,
                    Title = msg.Title,
                    TextContent = sb.ToString().Trim(),
                    MeanConfidence = meanConfidence,
                    ProcessedAt = DateTime.UtcNow
                };

                var resultJson = JsonSerializer.SerializeToUtf8Bytes(ocrResult);
                await _channel!.BasicPublishAsync(
                    exchange: "", routingKey: IndexQueueName, body: resultJson);

                _logger.LogInformation(
                    "OCR complete for DocId={Id}. Confidence: {Confidence:P1}. Published to index-queue.",
                    msg.DocumentId, meanConfidence);

                //5. Bereinige temp Dateien
                Directory.Delete(workDir, recursive: true);
                File.Delete(pdfPath);

                await _channel!.BasicAckAsync(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process message {Tag}.", ea.DeliveryTag);
                await _channel!.BasicNackAsync(ea.DeliveryTag, false, requeue: false);
            }
        };

        await _channel.BasicConsumeAsync(QueueName, autoAck: false, consumer: consumer);

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
        await Task.Delay(Timeout.Infinite, cts.Token);
    }

    private async Task<string> DownloadFromMinio(string objectName)
    {
        var pdfPath = Path.Combine(Path.GetTempPath(), $"download-{Guid.NewGuid():N}.pdf");
        using var fs = File.Create(pdfPath);

        await _minioClient.GetObjectAsync(new GetObjectArgs()
            .WithBucket(BucketName)
            .WithObject(objectName)
            .WithCallbackStream(stream => stream.CopyTo(fs)));

        return pdfPath;
    }

    public override void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
        base.Dispose();
    }
}