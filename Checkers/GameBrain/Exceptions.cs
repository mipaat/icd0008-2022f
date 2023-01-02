using Common;

namespace GameBrain;

public class NotAllowedException : Exception
{
    public NotAllowedException(string message) : base(message)
    {
    }
}

public class IllegalGameStateException : IllegalStateException
{
    public IllegalGameStateException(string message) : base("Unexpected game state: " + message)
    {
    }
}