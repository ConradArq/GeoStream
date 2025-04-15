using GeoStream.Api.Domain.Models.Entities;
using GeoStream.Api.Application.Configurations;
using System.Linq.Expressions;

namespace GeoStream.Api.Application.Mappings
{
    public class QueryProfile
    {
        public void Configure()
        {
            //TODO: Find alternatives to building the query with contains conditions as it can be heavy on the db as the history of sent notifications grows
            // Register configurations for Hub
            QueryProfileConfig.Register<Hub>(config =>
            {
                config.ForProperty("ToRecipients", (entityProperty, value) =>
                {
                    return BuildEmailContainsCondition(entityProperty, value);
                });
                config.ForProperty("CcRecipients", (entityProperty, value) =>
                {
                    return BuildEmailContainsCondition(entityProperty, value);
                });

                config.ForProperty("BccRecipients", (entityProperty, value) =>
                {
                    return BuildEmailContainsCondition(entityProperty, value);
                });
            });
        }

        private static Expression BuildEmailContainsCondition(Expression entityProperty, object value)
        {
            // Ensure 'value' is not null and split only if it contains a valid string
            if (value is not string emailList || string.IsNullOrWhiteSpace(emailList))
            {
                return Expression.Constant(false);
            }

            // Split email list into individual emails
            var emails = emailList.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            Expression emailContainsCondition = Expression.Constant(false);

            // Build the condition for each email in the list
            foreach (var email in emails)
            {
                var emailConstant = Expression.Constant(email.Trim());
                var containsExpression = Expression.Call(entityProperty, "Contains", null, emailConstant);
                emailContainsCondition = Expression.OrElse(emailContainsCondition, containsExpression);
            }

            return emailContainsCondition;
        }

        // Initialize mappings by calling Configure during application startup
        public static void InitializeMappings()
        {
            var profile = new QueryProfile();
            profile.Configure();
        }
    }
}
