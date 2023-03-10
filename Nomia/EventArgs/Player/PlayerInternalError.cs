using System;
using Emzi0767.Utilities;
using Nomia.Entities;

namespace Nomia.EventArgs.Player;

public class PlayerInternalError : AsyncEventArgs
{
    /// <summary>
    /// Exception that was thrown
    /// </summary>
    public Exception Exception { get; }
    
    public PlayerInternalError(Exception exception)
    {
        Exception = exception;
    }
}