using System;
using DSharpPlus.AsyncEvents;

namespace Nomia.EventArgs;

public class LavalinkClientExceptionEventArgs : AsyncEventArgs
{
    /// <summary>
    /// Exception that was thrown
    /// </summary>
    public Exception Exception { get; }
    public LavalinkClientExceptionEventArgs(Exception exception)
    {
        Exception = exception;
    }
}