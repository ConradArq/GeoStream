using GeoStream.Api.Domain.Attributes;

namespace GeoStream.Api.Helpers
{
    public static class TypeDiscoveryHelper
    {
        private static IEnumerable<Type>? _cachedEntities;

        /// <summary>
        /// Finds all types in the current AppDomain that are marked with the DiscoverableAttribute.
        /// </summary>
        /// <returns>A collection of types marked with the DiscoverableAttribute.</returns>
        public static IEnumerable<Type> GetDiscoverableEntities()
        {
            if (_cachedEntities == null)
            {
                _cachedEntities = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(a => a.GetTypes())
                    .Where(t => t.GetCustomAttributes(typeof(DiscoverableAttribute), false).Any());
            }
            return _cachedEntities;
        }
    }
}
