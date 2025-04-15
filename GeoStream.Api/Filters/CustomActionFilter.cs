using Microsoft.AspNetCore.Mvc.Filters;
using GeoStream.Api.Application.Exceptions;
using System.Reflection;

namespace GeoStream.Api.API.Filters
{
    /// <summary>
    /// A custom action filter that intercepts and processes action execution.
    /// This filter runs after authorization and model binding are completed, 
    /// allowing access to fully resolved action parameters and ensuring the user is authorized.
    /// 
    /// This filter performs two key operations:
    /// 1. Verifies the existence of entities for operations involving specific entities, ensuring that
    ///    the requested entity exists before proceeding.
    /// 2. Trims string properties in the action's arguments to remove leading and trailing whitespace,
    ///    maintaining clean and consistent data for processing.
    /// </summary>
    public class CustomActionFilter : IAsyncActionFilter
    {     
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (next == null) throw new ArgumentNullException(nameof(next));

            await VerifyEntityExistenceAsync(context);        
            TrimStringProperties(context.ActionArguments);

            await next();
        }

        // Verifies the existence of an entity based on its ID extracted from the current HTTP request. If an ID is found in the request,
        // a corresponding entity must exist; otherwise, the validation passes since the entity ID is not part of the request.
        private async Task VerifyEntityExistenceAsync(ActionExecutingContext context)
        {
            var httpContext = context.HttpContext;
            if (httpContext == null)
                throw new InvalidOperationException("HttpContext is not available.");

            object? entityId = await FilterHelper.GetIdFromRequestAsync(httpContext);

            if (entityId != null)
            {
                var httpContextAccessor = httpContext.RequestServices.GetService<IHttpContextAccessor>();
                if (httpContextAccessor == null)
                    throw new InvalidOperationException("IHttpContextAccessor is not registered in the service provider.");

                var serviceProvider = httpContext.RequestServices;
                if (serviceProvider == null)
                    throw new InvalidOperationException("ServiceProvider is not available.");

                var entity = await FilterHelper.GetEntityByAsync(httpContextAccessor, serviceProvider, null, entityId);

                if (entity == null)
                {
                    throw new NotFoundException(entityId);
                }
            }
        }

        private void TrimStringProperties(IDictionary<string, object?> parameters)
        {
            if (parameters == null) return;

            foreach (var parameter in parameters.Values)
            {
                if (parameter == null)
                    continue;

                var properties = parameter.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    .Where(p => p.PropertyType == typeof(string) && p.CanRead && p.CanWrite);

                foreach (var property in properties)
                {
                    var value = (string?)property.GetValue(parameter);
                    if (value != null)
                    {
                        property.SetValue(parameter, value.Trim());
                    }
                }
            }
        }
    }
}
