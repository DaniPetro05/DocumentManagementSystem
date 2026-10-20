namespace DocumentManagementSystem.Exceptions;

public class DocumentProcessingException : Exception
{
    public DocumentProcessingException(string message) : base(message) { }
    public DocumentProcessingException(string message, Exception inner) : base(message, inner) { }
}