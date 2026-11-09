using Minio;
using Minio.DataModel.Args;

namespace DocumentManagementSystem.Services;

public class MinioService
{
    private readonly IMinioClient _client;
    private const string BucketName = "documents";

    public MinioService(IConfiguration config)
    {
        _client = new MinioClient()
            .WithEndpoint(config["MINIO_ENDPOINT"] ?? "minio:9000")
            .WithCredentials(
                config["MINIO_ACCESS_KEY"] ?? "minioadmin",
                config["MINIO_SECRET_KEY"] ?? "minioadmin")
            .WithSSL(false)
            .Build();
        EnsureBucketExistsAsync().GetAwaiter().GetResult();
    }

    private async Task EnsureBucketExistsAsync()
    {
        var exists = await _client.BucketExistsAsync(new BucketExistsArgs().WithBucket(BucketName));
        if (!exists)
            await _client.MakeBucketAsync(new MakeBucketArgs().WithBucket(BucketName));
    }

    public async Task<string> UploadPdfAsync(int documentId, string title, Stream pdfStream)
    {
        var objectName = $"{documentId}/{title}.pdf";
        await _client.PutObjectAsync(new PutObjectArgs()
            .WithBucket(BucketName)
            .WithObject(objectName)
            .WithStreamData(pdfStream)
            .WithObjectSize(pdfStream.Length));
        return objectName;
    }

    public async Task<MemoryStream> DownloadPdfAsync(string objectName)
    {
        var ms = new MemoryStream();
        await _client.GetObjectAsync(new GetObjectArgs()
            .WithBucket(BucketName)
            .WithObject(objectName)
            .WithCallbackStream(stream => stream.CopyTo(ms)));
        ms.Position = 0;
        return ms;
    }

    public async Task DeletePdfAsync(string objectName)
    {
        await _client.RemoveObjectAsync(new RemoveObjectArgs()
            .WithBucket(BucketName)
            .WithObject(objectName));
    }
}