using GeoStream.Api.Application.Dtos;
using GeoStream.Api.Application.Dtos.Region;

namespace GeoStream.Api.Application.Interfaces.Services
{
    public interface IRegionService
    {
        Task<ResponseDto<IEnumerable<ResponseRegionDto>>> SearchAsync(SearchRegionDto requestDto);
    }
}