using Microsoft.AspNetCore.Mvc.Filters;
using GeoStream.Api.Application.Exceptions;
using GeoStream.Api.Domain.Enums;
using GeoStream.Api.Application.Resources;

namespace GeoStream.Api.API.Filters
{
    /// <summary>
    /// A custom action filter that validates the status of an entity based on the provided criteria.
    /// If the entity's status does not match one of the statuses specified in the <see cref="Statuses"/> array, 
    /// a conflict exception is thrown. This ensures that only entities in an allowed status can proceed with the action.
    /// 
    /// The filter runs later in the HTTP request pipeline, after authorization, model binding, 
    /// and other action filters, ensuring that the entity's status is validated before the action executes.
    /// </summary>
    public class ValidateEntityStatusAttribute : Attribute, IAsyncActionFilter
    {
        public Type EntityType { get; }
        public Status[] Statuses { get; }
        public string IdParameterName { get; }

        public ValidateEntityStatusAttribute(Type entityType, Status[] statuses, string idParameterName = "id")
        {
            EntityType = entityType;
            Statuses = statuses;
            IdParameterName = idParameterName;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var httpContext = context.HttpContext;
            if (httpContext == null)
                throw new InvalidOperationException("HttpContext is not available.");

            var httpContextAccessor = httpContext.RequestServices.GetService<IHttpContextAccessor>();
            if (httpContextAccessor == null)
                throw new InvalidOperationException("IHttpContextAccessor is not registered in the service provider.");

            var serviceProvider = httpContext.RequestServices;
            if (serviceProvider == null)
                throw new InvalidOperationException("ServiceProvider is not available.");

            var entity = await FilterHelper.GetEntityByAsync(httpContextAccessor, serviceProvider, EntityType);

            if (entity == null)
                throw new InvalidOperationException("Entity not found in the request.");

            var statusIdProperty = entity.GetType().GetProperty("StatusId");
            if (statusIdProperty == null)
                throw new InvalidOperationException("StatusId property not found on the entity type.");

            var statusId = statusIdProperty.GetValue(entity);
            if (statusId == null)
                throw new InvalidOperationException("StatusId cannot be null.");


            if (!Statuses.Contains((Status)statusId))
            {
                throw new ConflictException(GeneralMessages.InvalidEntityStatusMessage);
            }

            var result = await next();

            // Add logic after the action executes if needed
        }
    }
}
