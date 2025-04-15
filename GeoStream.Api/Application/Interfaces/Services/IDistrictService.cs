using GeoStream.Api.Application.Dtos;
using GeoStream.Api.Application.Dtos.District;

namespace GeoStream.Api.Application.Interfaces.Services
{
    public interface IDistrictService
    {
        Task<ResponseDto<IEnumerable<ResponseDistrictDto>>> SearchAsync(SearchDistrictDto requestDto);
    }
}