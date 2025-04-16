using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel;
using System.Reflection;

namespace GeoStream.Api.API.ModelBinders
{
    /// <summary>
    /// A query string binder implemented to ensure that error messages are localized and consistent with 
    /// the application's standardized error handling format. 
    /// 
    /// Model binders run after authorization has completed and before action filters and attributes are executed, 
    /// ensuring that action method parameters are fully bound and ready for use during the subsequent pipeline stages.
    /// </summary>
    public class QueryStringBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var query = bindingContext.HttpContext.Request.Query;
            var modelType = bindingContext.ModelType;

            // Handle simple types (int, string, bool, etc.)
            if (TypeDescriptor.GetConverter(modelType).CanConvertFrom(typeof(string)) && !modelType.IsClass)
            {
                if (query.TryGetValue(bindingContext.FieldName, out var queryValue))
                {
                    var value = queryValue.FirstOrDefault();
                    if (!string.IsNullOrEmpty(value))
                    {
                        try
                        {
                            var converter = TypeDescriptor.GetConverter(modelType);
                            var convertedValue = converter.ConvertFromString(value);
                            bindingContext.Result = ModelBindingResult.Success(convertedValue);
                        }
                        catch
                        {
                            bindingContext.ModelState.AddModelError(bindingContext.FieldName,
                                $"Invalid value '{value}' for parameter '{bindingContext.FieldName}'");
                            bindingContext.Result = ModelBindingResult.Failed();
                        }
                    }
                }

                return Task.CompletedTask;
            }

            // Handle complex types
            var modelInstance = Activator.CreateInstance(modelType);

            foreach (var prop in modelType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!prop.CanWrite)
                    continue;

                var name = prop.Name;

                if (query.TryGetValue(name, out var queryValue))
                {
                    var value = queryValue.FirstOrDefault();
                    if (!string.IsNullOrEmpty(value))
                    {
                        try
                        {
                            var converter = TypeDescriptor.GetConverter(prop.PropertyType);
                            if (converter.CanConvertFrom(typeof(string)))
                            {
                                var convertedValue = converter.ConvertFromString(value);
                                prop.SetValue(modelInstance, convertedValue);
                            }
                        }
                        catch
                        {
                            bindingContext.ModelState.AddModelError(name, $"Invalid value '{value}' for parameter '{name}'");
                        }
                    }
                }
            }

            bindingContext.Result = ModelBindingResult.Success(modelInstance);
            return Task.CompletedTask;
        }
    }
}
