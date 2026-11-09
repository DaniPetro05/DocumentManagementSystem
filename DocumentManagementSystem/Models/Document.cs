namespace DocumentManagementSystem.Models;

public class Document
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public required string Format { get; set; }
    public string MinioObjectPath { get; set; } = "";
}