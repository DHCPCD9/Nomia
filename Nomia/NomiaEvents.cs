using Microsoft.Extensions.Logging;

namespace Nomia;

public static class NomiaEvents
{
    
    public static EventId Intents { get; } = new EventId(1000, "Intents");
    public static EventId Lavalink { get; set; } = new EventId(1001, "Lavalink");
}

