namespace DocumentManagementSystem.Exceptions;

public class MessagingException : Exception
{
    public MessagingException(string message) : base(message) { }
    public MessagingException(string message, Exception inner) : base(message, inner) { }
}