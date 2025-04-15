using System.Net;

namespace GeoStream.Dtos
{
    public class ApiResponseDto<TData> : IApiResponseDto<TData>
    {
        public ApiResponseDto() { }
        public ApiResponseDto(HttpStatusCode statusCode, string? message, TData? data, List<string>? errors)
        {
            StatusCode = statusCode;
            Message = message;
            Data = data;
            Errors = errors;
        }

        public HttpStatusCode StatusCode { get; set; }

        /// <summary>
        /// A general message describing the result of the operation. Used for providing additional information when 
        /// the operation is successful, or when there is a minor issue that doesn’t warrant a full failure.
        /// </summary>
        public string? Message { get; set; }

        /// <summary>
        /// This contains the result of the request only if the operation was successful.
        /// </summary>
        public TData? Data { get; set; }

        /// <summary>
        /// A list of errors encountered during the operation when the operation is considered a failure
        /// </summary>
        public List<string>? Errors { get; set; }

        /// <summary>
        /// Returns true if the status code indicates a successful API request (200-299).
        /// </summary>
        public bool Succeeded => StatusCode >= HttpStatusCode.OK && StatusCode < HttpStatusCode.MultipleChoices;
    }
}
