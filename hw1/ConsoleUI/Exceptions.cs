namespace ConsoleUI;

public class RenderQueueFullException : Exception
{
    public RenderQueueFullException(string causeContent) : base($"Can't add '{causeContent}' to render queue!")
    {
    }
}