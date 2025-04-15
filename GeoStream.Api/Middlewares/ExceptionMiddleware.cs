using Microsoft.EntityFrameworkCore;
using GeoStream.Api.API.Dtos;
using GeoStream.Api.Application.Exceptions;
using System.Net;
using System.Text.Json;
using GeoStream.Api.Application.Resources;

namespace GeoStream.Api.API.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        /// <summary>
        /// Centralized exception middleware that handles all exceptions propagated within the HTTP pipeline.
        /// This middleware is responsible for creating appropriate HTTP responses by setting status codes, 
        /// formatting exception messages into standardized responses (e.g., JSON), and ensuring consistent error handling.
        /// Instead of logging errors at their origin, exceptions are thrown and captured here for centralized processing.
        /// </summary>
        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                if (ex is not NotFoundException && ex is not ValidationException)
                {
                    var exceptionMessages = new List<string>();
                    var currentException = ex;

                    while (currentException != null)
                    {
                        exceptionMessages.Add(currentException.Message);
                        currentException = currentException.InnerException;
                    }

                    _logger.LogError(ex, "An error occurred: {Messages}", string.Join(" -> ", exceptionMessages));
                }

                var result = string.Empty;
                JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions { Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

                switch (ex)
                {
                    case BadRequestException:
                        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                        result = JsonSerializer.Serialize(ApiResponseDto<object>.BadRequest(ex.Message), jsonSerializerOptions);
                        break;

                    case ValidationException validationException:
                        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                        var validationErrors = validationException.Errors.Select(e => $"{e.Key}: {string.Join(", ", e.Value)}").ToList();
                        result = JsonSerializer.Serialize(ApiResponseDto<object>.BadRequest(ex.Message, validationErrors), jsonSerializerOptions);
                        break;

                    case UnauthorizedAccessException:
                        context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                        result = JsonSerializer.Serialize(ApiResponseDto<object>.Unauthorized(ex.Message), jsonSerializerOptions);
                        break;

                    case ForbiddenException:
                        context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                        result = JsonSerializer.Serialize(ApiResponseDto<object>.Forbidden(ex.Message), jsonSerializerOptions);
                        break;

                    case NotFoundException:
                        context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                        result = JsonSerializer.Serialize(ApiResponseDto<object>.NotFound(ex.Message), jsonSerializerOptions);
                        break;

                    case ConflictException:
                        context.Response.StatusCode = (int)HttpStatusCode.Conflict;
                        result = JsonSerializer.Serialize(ApiResponseDto<object>.Conflict(ex.Message, null), jsonSerializerOptions);
                        break;

                    case DbUpdateException:
                        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        result = JsonSerializer.Serialize(ApiResponseDto<object>.InternalServerError(GeneralMessages.DbUpdateExceptionMessage), jsonSerializerOptions);
                        break;

                    default:
                        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        result = JsonSerializer.Serialize(ApiResponseDto<object>.InternalServerError(), jsonSerializerOptions);
                        break;
                }

                context.Response.ContentType = "application/json";

                await context.Response.WriteAsync(result);
            }
        }
    }
}
