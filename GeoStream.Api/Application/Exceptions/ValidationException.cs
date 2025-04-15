
using GeoStream.Api.Application.Resources;

namespace GeoStream.Api.Application.Exceptions
{
    /// <summary>
    /// Represents an exception that is intended to be returned in an API response with a status code of <see cref="HttpStatusCode.BadRequest"/>.
    /// This exception is typically used to indicate that one or more validation errors occurred during the processing of a request.
    /// </summary>
    public class ValidationException : ApplicationException
    {
        public ValidationException() : base(GeneralMessages.ValidationExceptionMessage)
        {
            Errors = new Dictionary<string, string[]>();
        }

        public ValidationException(string errorMessage) : this()
        {
            Errors = new Dictionary<string, string[]>
            {
                { "General", new[] { errorMessage } }
            };
        }

        public ValidationException(Dictionary<string, string[]> errors) : this()
        {
            Errors = errors;
        }

        public ValidationException(string key, string[] errors) : this()
        {
            Errors = new Dictionary<string, string[]>
            {
                { key, errors }
            };
        }

        public IDictionary<string, string[]> Errors { get; }
    }
}
