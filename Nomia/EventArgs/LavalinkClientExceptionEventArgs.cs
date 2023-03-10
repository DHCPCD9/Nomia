using System;
using Emzi0767.Utilities;

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