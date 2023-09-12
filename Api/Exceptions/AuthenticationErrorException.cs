using System;

namespace Api.Exceptions
{
    public class AuthenticationErrorException : Exception
    {
        public AuthenticationErrorException() : base("Invalid error occured.")
        {

        }
        public AuthenticationErrorException(string? message) : base(message)
        {

        }
        public AuthenticationErrorException(string? message, Exception? innerException) : base(message, innerException)
        {

        }
    }
}
