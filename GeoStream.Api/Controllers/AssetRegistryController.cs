using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GeoStream.Api.API.Dtos;
using GeoStream.Api.Application.Dtos.AssetRegistry;
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
    public class AssetRegistryController : Controller
    {
        private IAssetRegistryService _assetregistryService;

        public AssetRegistryController(IAssetRegistryService assetregistryService)
        {
            _assetregistryService = assetregistryService;
        }

        [HttpPost("search")]
        [Authorize(Roles = "Admin, Manager")]
        [ProducesResponseType(typeof(ApiResponseDto<IEnumerable<ResponseAssetRegistryDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> Search([FromBody] SearchAssetRegistryDto requestDto)
        {
            var responseDto = await _assetregistryService.SearchAsync(requestDto);
            var apiResponseDto = ApiResponseDto<IEnumerable<ResponseAssetRegistryDto>>.Ok(responseDto!);
            return Ok(apiResponseDto);
        }
    }
}
