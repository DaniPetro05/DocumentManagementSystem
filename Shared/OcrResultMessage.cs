public class OcrResultMessage
{
    public int DocumentId { get; set; }
    public string Title { get; set; } = "";
    public string TextContent { get; set; } = "";
    public double MeanConfidence { get; set; }
    public DateTime ProcessedAt { get; set; }
}