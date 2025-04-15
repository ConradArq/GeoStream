
using GeoStream.Api.Application.Resources;

namespace GeoStream.Api.Application.Exceptions
{
    /// <summary>
    /// Represents an exception that is intended to be returned in an API response with a status code of <see cref="HttpStatusCode.Conflict"/>.
    /// This exception is typically used when attempting to modify entities that are not in a valid state for the requested operation.
    /// </summary>
    public class ConflictException : ApplicationException
    {
        public ConflictException() : base(GeneralMessages.ConflictExceptionMessage)
        {
        }

        public ConflictException(string message) : base(message)
        {
        }
    }
}
