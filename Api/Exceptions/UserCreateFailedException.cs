using System;

namespace Api.Exceptions
{
    public class UserCreateFailedException : Exception
    {

        public UserCreateFailedException() : base("Something went wrong whilst creating user.")
        {

        }

        public UserCreateFailedException(string? message) : base(message)
        {
        }

        public UserCreateFailedException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
