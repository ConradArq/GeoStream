using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GeoStream.Api.API.Dtos;
using GeoStream.Api.Application.Dtos;
using GeoStream.Api.Application.Dtos.Route;
using GeoStream.Api.Application.Dtos.RouteHub;
using GeoStream.Api.Application.Dtos.Hub;
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
    public class RouteController : Controller
    {
        private IRouteService _routeService;

        public RouteController(IRouteService routeService)
        {
            _routeService = routeService;
        }

        [HttpPost("create")]
        [ProducesResponseType(typeof(ApiResponseDto<ResponseRouteDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> Create([FromBody] CreateRouteDto requestDto)
        {
            var responseDto = await _routeService.CreateAsync(requestDto);
            var apiResponseDto = ApiResponseDto<ResponseRouteDto>.Ok(responseDto!);
            return Ok(apiResponseDto);
        }

        [HttpPut("update/{id}")]
        [Authorize(Policy = "EntityOwnershipPolicy")]
        [ProducesResponseType(typeof(ApiResponseDto<ResponseRouteDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult> Update([FromRoute] int id, [FromBody] UpdateRouteDto requestDto)
        {
            var responseDto = await _routeService.UpdateAsync(id, requestDto);
            var apiResponseDto = ApiResponseDto<ResponseRouteDto>.Ok(responseDto!);
            return Ok(apiResponseDto);
        }

        [HttpPut("updaterange")]
        [ProducesResponseType(typeof(ApiResponseDto<ResponseRouteDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult> UpdateRange([FromBody] List<UpdateRouteDto> requestDto)
        {
            var responseDto = await _routeService.UpdateRangeAsync(requestDto);
            var apiResponseDto = ApiResponseDto<List<ResponseRouteDto>>.Ok(responseDto!);
            return Ok(apiResponseDto);
        }

        [HttpDelete("delete/{id}")]
        [Authorize(Policy = "EntityOwnershipPolicy")]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult> Delete([FromRoute] int id)
        {
            var responseDto = await _routeService.DeleteAsync(id);
            var apiResponseDto = ApiResponseDto<object>.Ok(responseDto!);
            return Ok(apiResponseDto);
        }

        [HttpGet("{id}")]
        [Authorize(Policy = "EntityOwnershipPolicy")]
        [ProducesResponseType(typeof(ApiResponseDto<ResponseRouteDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult> Get([FromRoute] int id)
        {
            var responseDto = await _routeService.GetAsync(id);
            var apiResponseDto = ApiResponseDto<ResponseRouteDto>.Ok(responseDto!);
            return Ok(apiResponseDto);
        }

        [HttpGet]
        [Authorize(Roles = "Admin, Manager")]
        [ProducesResponseType(typeof(ApiResponseDto<IEnumerable<ResponseRouteDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult> GetAll([FromQuery] RequestDto? requestDto)
        {
            var responseDto = await _routeService.GetAllAsync(requestDto);
            var apiResponseDto = ApiResponseDto<IEnumerable<ResponseRouteDto>>.Ok(responseDto!);
            return Ok(apiResponseDto);
        }

        [HttpPost("paginated")]
        [Authorize(Roles = "Admin, Manager")]
        [ProducesResponseType(typeof(ApiPaginatedResponseDto<IEnumerable<ResponseRouteDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> GetAllPaginated([FromBody] PaginationRequestDto requestDto)
        {
            var responseDto = await _routeService.GetAllPaginatedAsync(requestDto);
            var apiResponseDto = ApiPaginatedResponseDto<IEnumerable<ResponseRouteDto>>.Ok(responseDto!);
            return Ok(apiResponseDto);
        }

        [HttpPost("search")]
        [Authorize(Roles = "Admin, Manager")]
        [ProducesResponseType(typeof(ApiResponseDto<IEnumerable<ResponseRouteDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> Search([FromBody] SearchRouteDto requestDto)
        {
            var responseDto = await _routeService.SearchAsync(requestDto);
            var apiResponseDto = ApiResponseDto<IEnumerable<ResponseRouteDto>>.Ok(responseDto!);
            return Ok(apiResponseDto);
        }

        [HttpPost("searchpaginated")]
        [Authorize(Roles = "Admin, Manager")]
        [ProducesResponseType(typeof(ApiPaginatedResponseDto<IEnumerable<ResponseRouteDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> SearchPaginated([FromBody] SearchPaginatedRouteDto requestDto)
        {
            var responseDto = await _routeService.SearchPaginatedAsync(requestDto);
            var apiResponseDto = ApiPaginatedResponseDto<IEnumerable<ResponseRouteDto>>.Ok(responseDto!);
            return Ok(apiResponseDto);
        }

        [HttpGet("downloadkmzfile")]
        [Authorize(Roles = "Admin, Manager")]
        [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult?> DownloadKmzFile(int routeId)
        {
            var responseDto = await _routeService.GetKmzFileStreamResult(routeId);
            return responseDto?.Data; 
        }

        [HttpGet("getallroutehubs")]
        [Authorize(Roles = "Admin, Manager")]
        [ProducesResponseType(typeof(ApiResponseDto<IEnumerable<ResponseRouteHubDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult> GetAllRouteHubs([FromQuery] RequestDto? requestDto)
        {
            var responseDto = await _routeService.GetAllRouteHubsAsync(requestDto);
            var apiResponseDto = ApiResponseDto<IEnumerable<ResponseRouteHubDto>>.Ok(responseDto!);
            return Ok(apiResponseDto);
        }
    }
}
