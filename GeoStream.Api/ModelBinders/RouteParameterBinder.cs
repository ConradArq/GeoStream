using Microsoft.AspNetCore.Mvc.ModelBinding;
using GeoStream.Api.Application.Resources;

namespace GeoStream.Api.API.ModelBinders
{
    /// <summary>
    /// A route parameter binder implemented to ensure that error messages are localized and consistent with 
    /// the application's standardized error handling format. 
    /// 
    /// Model binders run after authorization has completed and before action filters and attributes are executed, 
    /// ensuring that action method parameters are fully bound and ready for use during the subsequent pipeline stages.
    /// </summary>
    public class RouteParameterBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var routeValue = bindingContext.HttpContext.Request.RouteValues[bindingContext.FieldName];
            if (routeValue != null)
            {
                try
                {
                    var converter = System.ComponentModel.TypeDescriptor.GetConverter(bindingContext.ModelType);
                    if (converter.CanConvertFrom(typeof(string)))
                    {
                        var convertedValue = converter.ConvertFromString(routeValue.ToString()!);
                        bindingContext.Result = ModelBindingResult.Success(convertedValue);
                    }
                }
                catch
                {
                    bindingContext.ModelState.AddModelError(bindingContext.FieldName, string.Format(ValidationMessages.InvalidRouteParameter, routeValue, bindingContext.ModelType.Name));
                    bindingContext.Result = ModelBindingResult.Failed();
                }
            }

            return Task.CompletedTask;
        }
    }
}
