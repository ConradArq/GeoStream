using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GeoStream.Api.API.Dtos;
using GeoStream.Api.Application.Dtos.District;
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
    public class DistrictController : Controller
    {
        private IDistrictService _districtService;

        public DistrictController(IDistrictService districtService)
        {
            _districtService = districtService;
        }

        [HttpPost("search")]
        [Authorize(Roles = "Admin, Manager")]
        [ProducesResponseType(typeof(ApiResponseDto<IEnumerable<ResponseDistrictDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> Search([FromBody] SearchDistrictDto requestDto)
        {
            var responseDto = await _districtService.SearchAsync(requestDto);
            var apiResponseDto = ApiResponseDto<IEnumerable<ResponseDistrictDto>>.Ok(responseDto!);
            return Ok(apiResponseDto);
        }
    }
}
