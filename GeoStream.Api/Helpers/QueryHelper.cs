using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using GeoStream.Api.Application.Configurations;
using GeoStream.Api.Application.Dtos;
using GeoStream.Api.Application.Exceptions;
using System.Linq.Dynamic.Core;
using GeoStream.Api.Application.Resources;
using MongoDB.Bson;

namespace GeoStream.Api.Helpers
{
    public static class QueryHelper
    {
        public enum StringComparisonMode
        {
            Contains,
            Equals
        }

        public enum LogicalOperation
        {
            And,
            Or
        }

        /// <summary>
        /// Builds a predicate based on the non-null properties of the provided search object,
        /// with configurable string comparison, logical operations, and support for custom comparison logic.
        /// </summary>
        /// <typeparam name="T">The type of the entity to filter.</typeparam>
        /// <param name="searchObj">The object containing the filter criteria.</param>
        /// <param name="stringComparisonMode">The mode of comparison for string properties (Contains or Equals).</param>
        /// <param name="propertyLogicalOperation">Logical operation between properties (AND or OR).</param>
        /// <param name="listElementLogicalOperation">Logical operation between elements in lists (AND or OR).</param>
        /// <returns>An expression that represents the filtering conditions for type T.</returns>
        public static Expression<Func<T, bool>> BuildPredicate<T>(
            object searchObj,
            StringComparisonMode stringComparisonMode = StringComparisonMode.Contains,
            LogicalOperation propertyLogicalOperation = LogicalOperation.And,
            LogicalOperation listElementLogicalOperation = LogicalOperation.Or)
        {
            var parameter = Expression.Parameter(typeof(T), "entity");

            Expression predicate = propertyLogicalOperation == LogicalOperation.And
                ? Expression.Constant(true)
                : Expression.Constant(false);

            var searchProperties = searchObj.GetType().GetProperties();
            var entityProperties = typeof(T).GetProperties();

            var mapping = QueryProfileConfig.GetMapping<T>();

            foreach (var property in searchProperties)
            {
                var hasIgnoreAttribute = property.GetCustomAttributes(inherit: false)
                    .Any(attr => attr.GetType().Name == "IgnoreInQueryPredicateAttribute");
                if (hasIgnoreAttribute) continue;

                var propertyValue = property.GetValue(searchObj);
                if (propertyValue == null) continue;

                var matchingEntityProperty = entityProperties.FirstOrDefault(p => p.Name == property.Name);
                if (matchingEntityProperty == null) continue;

                var entityProperty = Expression.Property(parameter, matchingEntityProperty.Name);

                if (mapping != null && mapping.TryGetCustomComparison(property.Name, out var customComparison))
                {
                    // Use custom comparison if configured in QueryMappingConfig
                    var customCondition = customComparison(entityProperty, propertyValue);
                    predicate = propertyLogicalOperation == LogicalOperation.And
                        ? Expression.AndAlso(predicate, customCondition)
                        : Expression.OrElse(predicate, customCondition);
                    continue;
                }

                Expression condition;
                var propertyType = matchingEntityProperty.PropertyType;

                // Handle list-based properties
                if (typeof(IEnumerable).IsAssignableFrom(property.PropertyType) && property.PropertyType != typeof(string))
                {
                    var searchValues = ((IEnumerable)propertyValue).Cast<object>().ToList();
                    if (!searchValues.Any()) continue;

                    Expression listCondition = listElementLogicalOperation == LogicalOperation.And
                        ? Expression.Constant(true)
                        : Expression.Constant(false);

                    var elementType = matchingEntityProperty.PropertyType.GetGenericArguments().FirstOrDefault();
                    if (elementType == null) continue;

                    foreach (var value in searchValues)
                    {
                        Expression elementCondition;
                        if (elementType == typeof(string) && propertyValue is string)
                        {
                            // Apply stringComparisonMode (Contains or Equals) for each element
                            var constantValue = Expression.Constant(value, typeof(string));
                            elementCondition = stringComparisonMode == StringComparisonMode.Contains
                                ? Expression.Call(entityProperty, "Contains", null, constantValue)
                                : Expression.Equal(entityProperty, constantValue);
                        }
                        else
                        {
                            // Use Equals for non-string elements
                            var constantValue = Expression.Constant(value, elementType);
                            elementCondition = Expression.Equal(entityProperty, constantValue);
                        }

                        // Combine each element condition based on listElementLogicalOperation (AND/OR)
                        listCondition = listElementLogicalOperation == LogicalOperation.And
                            ? Expression.AndAlso(listCondition, elementCondition)
                            : Expression.OrElse(listCondition, elementCondition);
                    }

                    condition = listCondition;
                }
                else if (property.PropertyType == typeof(string))
                {
                    // Apply stringComparisonMode (Contains or Equals) for string properties
                    var constantValue = Expression.Constant(propertyValue);
                    condition = stringComparisonMode == StringComparisonMode.Contains
                        ? Expression.Call(entityProperty, "Contains", null, constantValue)
                        : Expression.Equal(entityProperty, constantValue);
                }
                else
                {
                    // Use Equals for non-string, non-list properties
                    var constantValue = Expression.Constant(propertyValue);
                    condition = propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(Nullable<>)
                        ? Expression.Equal(entityProperty, Expression.Convert(constantValue, propertyType))
                        : Expression.Equal(entityProperty, constantValue);
                }

                // Combine condition based on propertyLogicalOperation (AND/OR)
                predicate = propertyLogicalOperation == LogicalOperation.And
                    ? Expression.AndAlso(predicate, condition)
                    : Expression.OrElse(predicate, condition);
            }

            // Return the final predicate as an expression lambda
            return Expression.Lambda<Func<T, bool>>(predicate, parameter);
        }

        /// <summary>
        /// Builds a dynamic <c>OrderBy</c> function for LINQ queries using the sorting information provided in a <see cref="RequestDto"/>.
        /// Returns <c>null</c> if no OrderBy field is specified.
        /// Validates that the specified OrderBy property exists on the entity type <typeparamref name="T"/>; otherwise, a <see cref="ValidationException"/> is thrown.
        /// This method is intended for use with API request models and performs request-level validation.
        /// </summary>
        /// <typeparam name="T">The entity type to apply the ordering to.</typeparam>
        /// <param name="requestDto">The request containing the OrderBy and OrderDirection values.</param>
        /// <returns>
        /// A delegate that applies ordering to an <see cref="IQueryable{T}"/> based on the request, or <c>null</c> if no OrderBy is set.
        /// </returns>
        /// <exception cref="ValidationException">
        /// Thrown when the specified OrderBy property does not exist on type <typeparamref name="T"/>, 
        /// indicating an invalid client request.
        /// </exception>
        public static Func<IQueryable<T>, IOrderedQueryable<T>>? BuildOrderByFunction<T>(QueryRequestDto? requestDto)
        {
            if (string.IsNullOrWhiteSpace(requestDto?.OrderBy))
                return null;

            var propertyName = requestDto.OrderBy.Trim();
            var sortDirection = string.Equals(requestDto.OrderDirection, "desc", StringComparison.OrdinalIgnoreCase)
                ? "descending"
                : "ascending";

            var prop = typeof(T).GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (prop == null)
            {
                throw new ValidationException(new Dictionary<string, string[]>
                {
                    { nameof(requestDto.OrderBy), new[] { string.Format(ValidationMessages.InvalidQueryParameter, requestDto.OrderBy, typeof(T).Name) } }
                });
            }

            return x => x.OrderBy($"{propertyName} {sortDirection}");
        }

        public static Func<IQueryable<T>, IQueryable<T>>[] Includes<T>(params Func<IQueryable<T>, IQueryable<T>>[] includes)
            where T : class => includes;      
    }
}
