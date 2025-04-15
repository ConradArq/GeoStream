namespace GeoStream.Api.Infrastructure.Interfaces.Services
{
    public interface IHttpService
    {
        Task<T> SendAsync<T>(
           HttpMethod method,
           string endpoint,
           object? payload = null,
           string? contentType = null,
           Dictionary<string, string>? headers = null,
           Dictionary<string, string>? queryParams = null);
    }
}
