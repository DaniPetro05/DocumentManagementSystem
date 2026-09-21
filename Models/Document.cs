namespace DocumentManagementSystem.Models;

public class Document
{
    public required int Id { get; set; }
    public required string Title { get; set; }
    public required string Format { get; set; }
}