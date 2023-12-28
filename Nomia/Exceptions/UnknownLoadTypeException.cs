using System;

namespace Nomia.Exceptions;

internal class UnknownLoadTypeException : Exception
{
    public UnknownLoadTypeException(string message) : base(message)
    {
    }
}