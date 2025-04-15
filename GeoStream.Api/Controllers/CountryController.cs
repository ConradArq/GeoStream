using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GeoStream.Api.API.Dtos;
using GeoStream.Api.Application.Dtos.Country;
using GeoStream.Api.Application.Interfaces.Services;

namespace GeoStream.Api.Controllers
{
    // Only authentication is required; no specific authorization policies or roles are applied.
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status500InternalServerError)]
    public class CountryController : Controller
    {
        private ICountryService _countryService;

        public CountryController(ICountryService countryService)
        {
            _countryService = countryService;
        }

        [HttpPost("search")]
        [Authorize(Roles = "Admin, Manager")]
        [ProducesResponseType(typeof(ApiResponseDto<IEnumerable<ResponseCountryDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> Search([FromBody] SearchCountryDto requestDto)
        {
            var responseDto = await _countryService.SearchAsync(requestDto);
            var apiResponseDto = ApiResponseDto<IEnumerable<ResponseCountryDto>>.Ok(responseDto!);
            return Ok(apiResponseDto);
        }
    }
}
