using System.Net;

namespace GeoStream.Dtos
{
    public interface IApiResponseDto
    {
        List<string>? Errors { get; set; }
        string? Message { get; set; }
        HttpStatusCode StatusCode { get; set; }
    }
    public interface IApiResponseDto<TData> : IApiResponseDto
    {
        TData? Data { get; set; }
    }
}