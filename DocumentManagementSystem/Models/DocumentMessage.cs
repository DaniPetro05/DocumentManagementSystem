namespace DocumentManagementSystem.Services;

public enum DocumentAction
{
    Created,
    Updated,
    Deleted
}

public record DocumentMessage
{
    public int DocumentId { get; init; }
    public string Title { get; init; } = "";
    public string Format { get; init; } = "";
    public DocumentAction Action { get; init; }
    public DateTime UploadedAt { get; init; } = DateTime.UtcNow;
    public string MinioObjectPath { get; set; } = "";
}