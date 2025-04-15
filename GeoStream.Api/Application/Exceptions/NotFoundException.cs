
using GeoStream.Api.Application.Resources;

namespace GeoStream.Api.Application.Exceptions
{
    /// <summary>
    /// Represents an exception that is intended to be returned in an API response with a status code of <see cref="HttpStatusCode.NotFound"/>.
    /// This exception is typically used when a requested resource or entity could not be found.
    /// </summary>
    public class NotFoundException : ApplicationException
    {
        public NotFoundException() : base(GeneralMessages.NotFoundExceptionMessage)
        {
        }

        public NotFoundException(object key) : base(string.Format(GeneralMessages.NotFoundExceptionEntityMessage, key))
        {
        }

        public NotFoundException(string msg) : base(msg)
        {
        }
    }
}
