using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace GeoStream.Api.API.ModelBinders.Providers
{
    public class RouteParameterBinderProvider : IModelBinderProvider
    {
        public IModelBinder? GetBinder(ModelBinderProviderContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            if (context.Metadata.BindingSource == BindingSource.Path)
            {
                return new RouteParameterBinder();
            }
            return null;
        }
    }
}
