using System.Security.Claims;
using GeoStream.Api.Infrastructure.Persistence.MSSQL;

namespace GeoStream.Api.API.Middlewares
{
    public class UserContextMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// Middleware responsible for setting the current user's ID in the DbContext.
        /// This ID is automatically assigned to the CreatedBy and LastModifiedBy properties of each entity during data operations.
        /// </summary>
        public UserContextMiddleware(RequestDelegate next, IHttpContextAccessor httpContextAccessor)
        {
            _next = next;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task InvokeAsync(HttpContext context, GeoStreamDbContext dbContext)
        {
            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "System";
            dbContext.CurrentUserId = userId;

            await _next(context);
        }
    }
}
