using GeoStream.Api.Application.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoStream.Api.Application.Exceptions
{
    /// <summary>
    /// Represents an exception that is intended to be returned in an API response with a status code of <see cref="HttpStatusCode.BadRequest"/>.
    /// This exception is typically used when the request contains invalid data or parameters.
    /// </summary>
    public class BadRequestException : ApplicationException
    {
        public BadRequestException() : base(GeneralMessages.BadRequestExceptionMessage)
        {
        }

        public BadRequestException(string message) : base(message)
        {
        }
    }
}
