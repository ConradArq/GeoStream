using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace GeoStream.Api.API.Filters
{
    /// <summary>
    /// Authorization handler that enforces access control based on entity ownership or specific access rules.
    /// Validates whether the requesting user has the required permissions to access or manipulate the entity identified by its ID.
    /// Admin and manager users always have access, bypassing ownership checks.
    /// 
    /// This handler runs early in the HTTP request pipeline, just after routing is resolved, ensuring access control 
    /// is verified before further processing such as model binding or action execution.
    /// </summary>
    public class EntityOwnershipHandler : AuthorizationHandler<EntityOwnershipRequirement>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IServiceProvider _serviceProvider;

        public EntityOwnershipHandler(IHttpContextAccessor httpContextAccessor, IServiceProvider serviceProvider)
        {
            _httpContextAccessor = httpContextAccessor;
            _serviceProvider = serviceProvider;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, EntityOwnershipRequirement requirement)
        {
            var entity = await FilterHelper.GetEntityByAsync(_httpContextAccessor, _serviceProvider, requirement.EntityType);
            // This handler is not concerned with the entity's existence, only with ownership of the entity in case it exists.
            if (entity == null)
            {
                context.Succeed(requirement);
                return;
            }

            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
                throw new InvalidOperationException("HttpContext is not available.");

            var user = httpContext.User;
            if (user == null || (!user.Identity?.IsAuthenticated ?? true))
                throw new InvalidOperationException("User is not authenticated.");

            var roles = user.FindAll(ClaimTypes.Role)?.Select(r => r.Value).ToList();
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (roles == null || userId == null)
                throw new InvalidOperationException("User roles or user ID could not be retrieved.");

            bool hasAccess = true;

            if (!roles.Contains("Admin") && !roles.Contains("Manager"))
            {
                var createdByProperty = entity.GetType().GetProperty("CreatedBy");
                if (createdByProperty == null)
                    throw new InvalidOperationException("CreatedBy property not found on the entity type.");

                var createdBy = createdByProperty.GetValue(entity)?.ToString();
                hasAccess = createdBy == userId;
            }

            if (hasAccess)
            {
                context.Succeed(requirement);
            }
            else
            {
                context.Fail();
            }
        }
    }

    public class EntityOwnershipRequirement : IAuthorizationRequirement
    {
        public Type? EntityType { get; }
        public string IdParameterName { get; }

        public EntityOwnershipRequirement(Type? entityType = null, string idParameterName = "id")
        {
            EntityType = entityType;
            IdParameterName = idParameterName;
        }
    }
}