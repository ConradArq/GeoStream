namespace GeoStream.Api.Domain.Attributes
{
    /// <summary>
    /// An attribute used to mark classes for dynamic discovery and processing at runtime.
    /// It can be applied to entities, services, or other components that need to be identified
    /// dynamically without requiring fully qualified names, such as namespaces and assemblies.
    /// 
    /// This simplifies runtime discovery and makes the code more adaptable to changes in
    /// assembly structure, namespaces, or class organization.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class DiscoverableAttribute : Attribute
    {
    }
}
