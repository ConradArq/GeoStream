using GeoStream.Dtos;

namespace GeoStream.Services
{
    public interface IApiClient
    {
        Task<TResponse> SendRequest<TResponse>(string key, object? content = null, 
            Dictionary<string, string>? pathParams = null, Dictionary<string, string>? queryParams = null) 
            where TResponse : IApiResponseDto, new();
    }
}
