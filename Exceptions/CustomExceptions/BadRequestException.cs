using System;

namespace dailycue_api.Exceptions.CustomExceptions;

public class BadRequestException : Exception
{
    public BadRequestException(string message) : base(message)
    {
    }
}