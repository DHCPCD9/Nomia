using System;
using System.Threading.Tasks;

namespace Nomia.Websocket.Entities;

public interface INomiaHandlerEntry
{
    Type Type { get; }
    Func<object, Task> Handler { get; }
}

public class NomiaHandlerEntry<T> : INomiaHandlerEntry
{
    public Func<object, Task> Handler { get; set; }
    public Type Type { get; } = typeof(T);
}