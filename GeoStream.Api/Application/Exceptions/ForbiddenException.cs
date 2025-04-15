using GeoStream.Api.Application.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoStream.Api.Application.Exceptions
{
    /// <summary>
    /// Represents an exception that is intended to be returned in an API response with a status code of <see cref="HttpStatusCode.Forbidden"/>.
    /// This exception is typically used when a user is authenticated but does not have permission to access a resource or perform an action.
    /// </summary>
    public class ForbiddenException : ApplicationException
    {
        public ForbiddenException() : base(GeneralMessages.ForbiddenExceptionMessage)
        {
        }

        public ForbiddenException(string message) : base(message)
        {
        }
    }
}
