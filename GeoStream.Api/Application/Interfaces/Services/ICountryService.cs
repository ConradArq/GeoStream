using GeoStream.Api.Application.Dtos;
using GeoStream.Api.Application.Dtos.Country;

namespace GeoStream.Api.Application.Interfaces.Services
{
    public interface ICountryService
    {
        Task<ResponseDto<IEnumerable<ResponseCountryDto>>> SearchAsync(SearchCountryDto requestDto);
    }
}