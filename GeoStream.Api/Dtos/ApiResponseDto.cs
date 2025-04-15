using System.Net;
using GeoStream.Api.Application.Dtos;
using GeoStream.Api.Application.Resources;

namespace GeoStream.Api.API.Dtos
{
    public class ApiResponseDto<TData>
    {
        public ApiResponseDto(HttpStatusCode statusCode, string? message, TData? data, List<string>? errors)
        {
            StatusCode = statusCode;
            Message = message;
            Data = data;
            Errors = errors;
        }

        public HttpStatusCode StatusCode { get; }

        /// <summary>
        /// A general message describing the result of the operation. Used for providing additional information when 
        /// the operation is successful, or when there is a minor issue that doesn’t warrant a full failure.
        /// </summary>
        public string? Message { get; }

        /// <summary>
        /// This contains the result of the request only if the operation was successful.
        /// </summary>
        public TData? Data { get; }

        /// <summary>
        /// A list of errors encountered during the operation when the operation is considered a failure
        /// </summary>
        public List<string>? Errors { get; }

        public static ApiResponseDto<TData> Ok(ResponseDto<TData?> responseDto)
        {
            return new ApiResponseDto<TData>(
                HttpStatusCode.OK,
                responseDto.Message ?? GeneralHttpStatusMessages.OperationCompletedSuccessfully,
                responseDto.Data,
                null
            );
        }

        public static ApiResponseDto<TData> Accepted(ResponseDto<TData?> responseDto)
        {
            return new ApiResponseDto<TData>(
                HttpStatusCode.Accepted,
                responseDto.Message ?? GeneralHttpStatusMessages.OperationAccepted,
                responseDto.Data,
                null
            );
        }

        public static ApiResponseDto<TData> BadRequest(string? message = null, List<string>? errors = null)
        {
            return new ApiResponseDto<TData>(
                HttpStatusCode.BadRequest,
                message ?? GeneralHttpStatusMessages.BadRequest,
                default,
                errors
            );
        }

        public static ApiResponseDto<TData> Unauthorized(string? message = null)
        {
            return new ApiResponseDto<TData>(
                HttpStatusCode.Unauthorized,
                message ?? GeneralHttpStatusMessages.Unauthorized,
                default,
                null
            );
        }

        public static ApiResponseDto<TData> Forbidden(string? message = null)
        {
            return new ApiResponseDto<TData>(
                HttpStatusCode.Forbidden,
                message ?? GeneralHttpStatusMessages.Forbidden,
                default,
                null
            );
        }

        public static ApiResponseDto<TData> NotFound(string? message = null)
        {
            return new ApiResponseDto<TData>(
                HttpStatusCode.NotFound,
                message ?? GeneralHttpStatusMessages.NotFound,
                default,
                null
            );
        }

        public static ApiResponseDto<TData> Conflict(string? message = null, List<string>? errors = null)
        {
            return new ApiResponseDto<TData>(
                HttpStatusCode.Conflict,
                message ?? GeneralHttpStatusMessages.Conflict,
                default,
                errors
            );
        }

        public static ApiResponseDto<TData> InternalServerError(string? message = null)
        {
            return new ApiResponseDto<TData>(
                HttpStatusCode.InternalServerError,
                message ?? GeneralHttpStatusMessages.InternalServerError,
                default,
                null
            );
        }
    }
}