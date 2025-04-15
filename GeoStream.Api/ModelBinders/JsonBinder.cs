using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Text.Json;
using GeoStream.Api.Application.Resources;

namespace GeoStream.Api.API.ModelBinders
{
    /// <summary>
    /// A Json model binder implemented to ensure that error messages for deserialization and binding issues 
    /// are localized and consistent with the application's standardized error handling format. 
    /// 
    /// The JsonSerializerOptions are customized to allow case-insensitivity, skipping comments, 
    /// allowing trailing commas, and reading numbers from strings.
    /// 
    /// Model binders run after authorization has completed and before action filters and attributes are executed, 
    /// ensuring that action method parameters are fully bound and ready for use during the subsequent pipeline stages.
    /// </summary>
    public class JsonBinder : IModelBinder
    {
        public async Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var httpContext = bindingContext.HttpContext;
            string? contentType = httpContext.Request.ContentType;

            if (!string.IsNullOrEmpty(contentType) && contentType.StartsWith("application/json", StringComparison.OrdinalIgnoreCase))
            {
                // The httpContext.Request.Body is a non-seekable HttpRequestStream. Once read, it cannot be read again without buffering.
                // To allow multiple reads of the body (e.g., in action filters, model binders, and attributes), we enable buffering 
                // on the request (in case it has not already been enabled) and reset the body's position to 0.
                httpContext.Request.EnableBuffering();
                httpContext.Request.Body.Position = 0;

                var requestBody = await new StreamReader(httpContext.Request.Body).ReadToEndAsync();
                if (!string.IsNullOrWhiteSpace(requestBody))
                {
                    try
                    {
                        var options = new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true,
                            AllowTrailingCommas = true,
                            ReadCommentHandling = JsonCommentHandling.Skip,
                            NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString
                        };

                        var model = JsonSerializer.Deserialize(requestBody, bindingContext.ModelType, options);
                        bindingContext.Result = ModelBindingResult.Success(model);
                    }
                    catch (JsonException ex)
                    {
                        string propertyName = ex.Path?.TrimStart('$', '.').Length > 0
                            ? char.ToUpper(ex.Path.TrimStart('$', '.')[0]) + ex.Path.TrimStart('$', '.').Substring(1)
                            : "$";

                        bindingContext.ModelState.AddModelError(propertyName, string.Format(propertyName == "$" ? ValidationMessages.InvalidJsonFormat : ValidationMessages.InvalidJsonPropertyFormat, ex.Message));
                        bindingContext.Result = ModelBindingResult.Failed();
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"Error deserializing '{requestBody}': {ex.Message}", ex);
                    }
                }
                else
                {
                    bindingContext.ModelState.AddModelError("$", ValidationMessages.EmptyRequestBody);
                    bindingContext.Result = ModelBindingResult.Failed();
                }
            }
            else
            {
                bindingContext.Result = ModelBindingResult.Failed();
            }
        }
    }
}
