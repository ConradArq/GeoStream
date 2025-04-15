using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GeoStream.Api.API.Dtos;
using GeoStream.Api.Application.Dtos.Emitter;
using GeoStream.Api.Application.Services;

namespace GeoStream.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status500InternalServerError)]
    public class EmitterController : Controller
    {
        private readonly IEmitterService _emitterService;

        public EmitterController(IEmitterService emitterService)
        {
            _emitterService = emitterService;
        }

        [HttpPost("searchpaginated")]
        [Authorize(Roles = "Admin, Manager")]
        [ProducesResponseType(typeof(ApiPaginatedResponseDto<IEnumerable<ResponseEmitterDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> SearchPaginated([FromBody] SearchPaginatedEmitterDto requestDto)
        {
            var responseDto = await _emitterService.SearchPaginatedAsync(requestDto);
            var apiResponseDto = ApiPaginatedResponseDto<IEnumerable<ResponseEmitterDto>>.Ok(responseDto!);
            return Ok(apiResponseDto);
        }

        [HttpPost("searchincidentpaginated")]
        [Authorize(Roles = "Admin, Manager")]
        [ProducesResponseType(typeof(ApiPaginatedResponseDto<IEnumerable<ResponseIncidentEmitterDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> SearchIncidentsPaginated([FromBody] SearchPaginatedEmitterDto requestDto)
        {
            var responseDto = await _emitterService.SearchIncidentsPaginatedAsync(requestDto);
            var apiResponseDto = ApiPaginatedResponseDto<IEnumerable<ResponseIncidentEmitterDto>>.Ok(responseDto!);
            return Ok(apiResponseDto);
        }

        [HttpPost("getincidentsintervalstatistics")]
        [Authorize(Roles = "Admin, Manager")]
        [ProducesResponseType(typeof(ApiResponseDto<IEnumerable<IncidentsIntervalStatisticsDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> GetIncidentsIntervalStatistics([FromBody] Interval requestDto)
        {
            var responseDto = await _emitterService.GetIncidentsIntervalStatisticsAsync(requestDto);
            var apiResponseDto = ApiResponseDto<IEnumerable<IncidentsIntervalStatisticsDto>>.Ok(responseDto!);
            return Ok(apiResponseDto);
        }

        [HttpGet("getallemitterssince")]
        [Authorize(Roles = "Admin, Manager")]
        [ProducesResponseType(typeof(ApiResponseDto<IEnumerable<ResponseEmitterDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult> GetAllEmittersSince([FromQuery] DateTime fromUtc)
        {
            var responseDto = await _emitterService.GetAllEmittersSinceAsync(fromUtc);
            var apiResponseDto = ApiResponseDto<IEnumerable<ResponseEmitterDto>>.Ok(responseDto!);
            return Ok(apiResponseDto);
        }

        [HttpGet("getallincidentssince")]
        [Authorize(Roles = "Admin, Manager")]
        [ProducesResponseType(typeof(ApiResponseDto<IEnumerable<ResponseIncidentEmitterDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult> GetAllIncidentsSince([FromQuery] DateTime fromUtc)
        {
            var responseDto = await _emitterService.GetAllIncidentsSinceAsync(fromUtc);
            var apiResponseDto = ApiResponseDto<IEnumerable<ResponseIncidentEmitterDto>>.Ok(responseDto!);
            return Ok(apiResponseDto);
        }
    }
}