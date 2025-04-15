using Microsoft.AspNetCore.Mvc.Controllers;
using System.Text.Json;
using GeoStream.Api.Domain.Interfaces.Repositories;
using System.Reflection;
using GeoStream.Api.Domain.Interfaces.Models;
using GeoStream.Api.Helpers;

/// <summary>
/// Provides utility methods for filters, including handling HTTP context,
/// extracting entity IDs, and retrieving repositories dynamically.
/// </summary>
public static class FilterHelper
{
    /// <summary>
    /// Retrieves an entity based on its type and ID.
    /// </summary>
    /// <param name="httpContextAccessor">Provides access to the current HTTP context.</param>
    /// <param name="serviceProvider">The service provider used to resolve services required for retrieving the entity.</param>
    /// <param name="entityType"> The type of the entity to retrieve. If not provided, it is inferred from the controller name. </param>
    /// <param name="entityIdValue"> The specific value of the entity ID. If provided, this takes precedence over the value 
    /// extracted using <paramref name="entityIdKey"/>.
    /// </param>
    /// <param name="entityIdKey">
    /// Specifies the key used to locate the entity ID in the request. The value can be retrieved from 
    /// route values, query parameters, form data, or the JSON request body. Defaults to "id".
    /// </param>
    /// <returns>The retrieved entity object, or <c>null</c> if no matching entity is found.</returns>
    public static async Task<object?> GetEntityByAsync(
        IHttpContextAccessor httpContextAccessor,
        IServiceProvider serviceProvider,
        Type? entityType = null,
        object? entityIdValue = null,
        string entityIdKey = "id")
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext == null) throw new InvalidOperationException("HttpContext is not available.");

        var controllerActionDescriptor = httpContext.GetEndpoint()?.Metadata.GetMetadata<ControllerActionDescriptor>();
        if (controllerActionDescriptor == null) throw new InvalidOperationException("ControllerActionDescriptor is not available.");

        // Determine the entity type
        var resolvedEntityType = entityType ?? TypeDiscoveryHelper.GetDiscoverableEntities().FirstOrDefault(x => string.Equals(x.Name, controllerActionDescriptor.ControllerName.Replace("Controller", ""), StringComparison.OrdinalIgnoreCase));
        if (resolvedEntityType == null) throw new InvalidOperationException($"Could not resolve entity type for '{controllerActionDescriptor.ControllerName}'.");

        // Extract the entity ID
        object? searchEntityId = entityIdValue ?? await GetIdFromRequestAsync(httpContext, entityIdKey);
        if (searchEntityId == null) return null;

        using var scope = serviceProvider.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var repository = unitOfWork.GetRepository(resolvedEntityType);
        // Alternative way to retrieve the repository dynamically
        //var repository = GetGenericRepository(unitOfWork, resolvedEntityType, null);

        // Get non-generic GetSingleAsync method from the resolved repository for the resolved entity type
        var getSingleAsyncMethod = repository.GetType().GetMethods()
            .FirstOrDefault(m => m.Name == "GetSingleAsync" && !m.IsGenericMethodDefinition);
        if (getSingleAsyncMethod == null) throw new InvalidOperationException($"GetSingleAsync method not found in repository for type '{resolvedEntityType.Name}'.");

        // Get the ID property from the resolved entity type
        PropertyInfo? resolvedEntityIdProperty = resolvedEntityType.GetProperty("Id");
        if (resolvedEntityIdProperty == null)
            throw new InvalidOperationException($"Entity {resolvedEntityType.Name} does not have an 'Id' property.");

        // Compare the ID from the request or method parameters with the entity's ID type, returning null if they are incompatible
        if (searchEntityId != null && !resolvedEntityIdProperty.PropertyType.IsInstanceOfType(searchEntityId))
        {
            // Parse searchEntityId to int if necessary, as it may come from an HTTP request or another source as a string
            if (int.TryParse(searchEntityId.ToString(), out int parsedId) && resolvedEntityIdProperty.PropertyType == typeof(int))
            {
                searchEntityId = parsedId;
            }
            else
            {
                return null;
            }
        }

        Type queryableType = typeof(IQueryable<>).MakeGenericType(resolvedEntityType);
        Type funcType = typeof(Func<,>).MakeGenericType(queryableType, queryableType);
        Array includesArray = Array.CreateInstance(funcType, 0);

        // Invoke the GetSingleAsync method dynamically
        var task = (Task?)getSingleAsyncMethod.Invoke(repository, new object?[] { searchEntityId, false, includesArray });
        var taskType = typeof(Task<>).MakeGenericType(resolvedEntityType);

        if (task == null || !taskType.IsInstanceOfType(task))
            throw new InvalidOperationException("GetSingleAsync did not return a valid Task.");

        await task;
        // Extract the result from completed task
        var resultProperty = task!.GetType().GetProperty("Result");
        var entity = resultProperty?.GetValue(task);
        // Alternative approach: Dynamically cast the task result to retrieve the entity
        // dynamic entity = await (dynamic)task;

        return entity;
    }

    /// <summary>
    /// Retrieves an ID value from the HTTP request. 
    /// The method searches for the ID in the following order: route values, query parameters, form data, 
    /// or the JSON request body. The source of the ID depends on the request context:
    /// - For route values or query parameters, the ID is extracted directly from the URL or query string.
    /// - For form data, the ID is extracted from the request body (e.g., form submissions).
    /// - For JSON request bodies, the ID is extracted from the body content.
    /// </summary>
    /// <param name="httpContext">
    /// The HTTP context containing the current request, which provides access to route values, 
    /// query parameters, form data, and the request body.
    /// </param>
    /// <param name="entityIdKey">
    /// The name of the key used to locate the ID in the request. This key is searched for in 
    /// route values, query parameters, form data, or the JSON request body. Defaults to "id".
    /// </param>
    /// <returns>
    /// The retrieved ID value if found, or <c>null</c> if no matching value is present in the request.
    /// </returns>
    public static async Task<object?> GetIdFromRequestAsync(HttpContext httpContext, string entityIdKey = "id")
    {
        // Try to retrieve the entity ID from RouteValues
        if (httpContext.Request.RouteValues.TryGetValue(entityIdKey, out var routeValue))
        {
            return routeValue;
        }

        // Try to retrieve the entity ID from Query parameters
        if (httpContext.Request.Query.TryGetValue(entityIdKey, out var queryValue))
        {
            return queryValue;
        }

        // Try to retrieve the entity ID from form data (e.g., application/x-www-form-urlencoded or multipart/form-data)
        if (httpContext.Request.HasFormContentType && httpContext.Request.Form.TryGetValue(entityIdKey, out var formValue))
        {
            return formValue;
        }

        // Try to retrieve the entity ID from the JSON Request Body for certain HTTP methods (POST, PUT, DELETE)
        if (httpContext.Request.Method == HttpMethods.Post || httpContext.Request.Method == HttpMethods.Put || httpContext.Request.Method == HttpMethods.Delete)
        {
            var bodyEntityId = await ExtractIdFromBodyAsync(httpContext.Request, entityIdKey);
            return bodyEntityId;
        }

        return null;
    }

    private static async Task<string?> ExtractIdFromBodyAsync(HttpRequest request, string entityIdKey)
    {
        // The httpContext.Request.Body is a non-seekable HttpRequestStream. Once read, it cannot be read again without buffering.
        // To allow multiple reads of the body (e.g., in action filters, model binders, and attributes), we enable buffering 
        // on the request (in case it has not already been enabled) and reset the body's position to 0.
        request.EnableBuffering();
        request.Body.Position = 0;

        var body = await new StreamReader(request.Body).ReadToEndAsync();

        if (!string.IsNullOrWhiteSpace(body))
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true,
                NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString
            };

            try
            {
                var jsonObject = JsonSerializer.Deserialize<Dictionary<string, object>>(body, options);

                if (jsonObject != null && jsonObject.TryGetValue(entityIdKey, out var idValue))
                {
                    return idValue switch
                    {
                        string strValue => strValue,
                        JsonElement jsonElement => jsonElement.ValueKind switch
                        {
                            JsonValueKind.String => jsonElement.GetString(),
                            JsonValueKind.Number => jsonElement.GetRawText(),
                            _ => null
                        },
                        _ => idValue.ToString()
                    };
                }
            }
            catch
            {
                /// Deserialization errors are handled in <see cref="NotificationService.API.ModelBinders.JsonBinder"/>
            }
        }

        return null;
    }

    /// <summary>
    /// Retrieves a generic repository for the specified entity type or entity name.
    /// Either <paramref name="entityType"/> or <paramref name="entityName"/> must be provided.
    /// </summary> 
    private static object GetGenericRepository(IUnitOfWork unitOfWork, Type? entityType, string? entityName)
    {
        if (entityType == null)
        {
            if (string.IsNullOrEmpty(entityName))
                throw new ArgumentNullException(nameof(entityName), "Entity name must be provided if entity type is null.");

            // Resolve the entity type from the assembly using the entity name
            var entityTypeAssembly = typeof(IBaseDomainModel).Assembly;
            entityType = entityTypeAssembly.GetTypes()
                .FirstOrDefault(t => t.Name.Equals(entityName, StringComparison.OrdinalIgnoreCase));

            if (entityType == null)
                throw new InvalidOperationException($"Entity type for name '{entityName}' could not be found.");
        }

        // Dynamically resolve the repository method for the specified entity type
        var repositoryMethod = typeof(IUnitOfWork).GetMethod("Repository")?.MakeGenericMethod(entityType);
        if (repositoryMethod == null)
            throw new InvalidOperationException("Could not resolve the Repository method.");

        // Invoke the repository method to get the repository instance
        var repository = repositoryMethod.Invoke(unitOfWork, null);
        if (repository == null)
            throw new InvalidOperationException($"Repository for type '{entityType.Name}' could not be created.");

        return repository;
    }
}