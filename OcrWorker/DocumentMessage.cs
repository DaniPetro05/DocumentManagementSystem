namespace OcrWorker;

public record DocumentMessage
{
    public int DocumentId { get; init; }
    public string Title { get; init; } = "";
    public string Format { get; init; } = "";
    public DateTime UploadedAt { get; init; }
}