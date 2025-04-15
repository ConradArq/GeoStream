using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GeoStream.Api.API.Dtos;
using GeoStream.Api.Application.Dtos.Region;
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
    public class RegionController : Controller
    {
        private IRegionService _regionService;

        public RegionController(IRegionService regionService)
        {
            _regionService = regionService;
        }

        [HttpPost("search")]
        [Authorize(Roles = "Admin, Manager")]
        [ProducesResponseType(typeof(ApiResponseDto<IEnumerable<ResponseRegionDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> Search([FromBody] SearchRegionDto requestDto)
        {
            var responseDto = await _regionService.SearchAsync(requestDto);
            var apiResponseDto = ApiResponseDto<IEnumerable<ResponseRegionDto>>.Ok(responseDto!);
            return Ok(apiResponseDto);
        }
    }
}
