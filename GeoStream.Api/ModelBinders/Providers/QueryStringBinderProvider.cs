using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace GeoStream.Api.API.ModelBinders.Providers
{
    public class QueryStringBinderProvider : IModelBinderProvider
    {
        public IModelBinder? GetBinder(ModelBinderProviderContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            if (context.Metadata.BindingSource == BindingSource.Query)
            {
                return new QueryStringBinder();
            }
            return null;
        }
    }
}
