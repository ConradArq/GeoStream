using AutoMapper;
using GeoStream.Api.Application.Dtos;
using GeoStream.Api.Application.Dtos.Emitter;
using GeoStream.Api.Domain.Interfaces.Repositories;
using MongoDB.Driver;
using GeoStream.Api.Domain.Enums;

namespace GeoStream.Api.Application.Services
{
    public class EmitterService : IEmitterService
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;

        public EmitterService(IMapper mapper, IUnitOfWork unitOfWork)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }

        public async Task<PaginatedResponseDto<IEnumerable<ResponseEmitterDto>>> SearchPaginatedAsync(SearchPaginatedEmitterDto requestDto)
        {
            var filter = Builders<ResponseEmitterDto>.Filter.Empty;
            var sort = Builders<ResponseEmitterDto>.Sort.Descending(x => x.DateTime);

            var result = await _unitOfWork.EmitterRepository.SearchPaginatedAsync(requestDto, sort, filter);
            var responseDtos = _mapper.Map<IEnumerable<ResponseEmitterDto>>(result.Data);

            return new PaginatedResponseDto<IEnumerable<ResponseEmitterDto>>(responseDtos, requestDto.PageNumber, requestDto.PageSize, result.TotalItems);
        }

        public async Task<PaginatedResponseDto<IEnumerable<ResponseIncidentEmitterDto>>> SearchIncidentsPaginatedAsync(SearchPaginatedEmitterDto requestDto)
        {
            var filter = Builders<ResponseIncidentEmitterDto>.Filter.Empty;
            var sort = Builders<ResponseIncidentEmitterDto>.Sort.Descending(x => x.DateTime);

            var result = await _unitOfWork.EmitterRepository.SearchIncidentsPaginatedAsync(requestDto, sort, filter);
            var responseDtos = _mapper.Map<IEnumerable<ResponseIncidentEmitterDto>>(result.Data);

            return new PaginatedResponseDto<IEnumerable<ResponseIncidentEmitterDto>>(responseDtos, requestDto.PageNumber, requestDto.PageSize, result.TotalItems);
        }

        public async Task<ResponseDto<IEnumerable<ResponseEmitterDto>>> GetAllEmittersSinceAsync(DateTime fromUtc)
        {
            var result = await _unitOfWork.EmitterRepository.GetAllEmittersSinceAsync(fromUtc);
            var responseDtos = _mapper.Map<IEnumerable<ResponseEmitterDto>>(result);
            return new ResponseDto<IEnumerable<ResponseEmitterDto>>(responseDtos);
        }

        public async Task<ResponseDto<IEnumerable<ResponseIncidentEmitterDto>>> GetAllIncidentsSinceAsync(DateTime fromUtc)
        {
            var result = await _unitOfWork.EmitterRepository.GetAllIncidentsSinceAsync(fromUtc);
            var responseDtos = _mapper.Map<IEnumerable<ResponseIncidentEmitterDto>>(result);
            return new ResponseDto<IEnumerable<ResponseIncidentEmitterDto>>(responseDtos);
        }

        public async Task<ResponseDto<IEnumerable<IncidentsIntervalStatisticsDto>>> GetIncidentsIntervalStatisticsAsync(Interval interval)
        {
            DateTime endDate = DateTime.UtcNow;
            DateTime startDate = interval switch
            {
                Interval.Daily => new DateTime(endDate.Year, endDate.Month, 1),
                Interval.Monthly => new DateTime(endDate.Year, 1, 1),
                Interval.Yearly => endDate.AddYears(-9),
                _ => throw new ArgumentOutOfRangeException()
            };

            var incidents = await _unitOfWork.EmitterRepository.GetIncidentsForIntervalAsync(startDate, endDate);

            var grouped = incidents
                .GroupBy(a => new
                {
                    IntervalKey = interval switch
                    {
                        Interval.Daily => a.DateTime.Day,
                        Interval.Monthly => a.DateTime.Month,
                        Interval.Yearly => a.DateTime.Year,
                        _ => throw new ArgumentOutOfRangeException()
                    },
                    a.IncidentType
                })
                .Select(g => new
                {
                    Emitter = g.Key.IntervalKey.ToString(),
                    IncidentType = g.Key.IncidentType,
                    Count = g.Count()
                })
                .ToList();

            var incidentTypes = Enum.GetValues(typeof(IncidentType)).Cast<IncidentType>().ToList();
            List<string> intervals = interval switch
            {
                Interval.Daily => Enumerable.Range(1, DateTime.DaysInMonth(endDate.Year, endDate.Month)).Select(d => d.ToString()).ToList(),
                Interval.Monthly => new[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" }.ToList(),
                Interval.Yearly => Enumerable.Range(endDate.Year - 9, 10).Select(y => y.ToString()).ToList(),
                _ => throw new ArgumentOutOfRangeException()
            };

            var stats = incidentTypes.Select(incidentType => new IncidentsTypeStatisticsDto
            {
                IncidentType = incidentType,
                Name = incidentType.ToString(),
                IncidentIntervalStatisticsDtos = intervals.Select((emitter, idx) => new IncidentIntervalStatisticsDto
                {
                    Emitter = emitter,
                    IncidentCount = interval == Interval.Monthly
                        ? grouped.Where(g => g.IncidentType == incidentType && g.Emitter == (idx + 1).ToString()).Select(g => g.Count).FirstOrDefault()
                        : grouped.Where(g => g.IncidentType == incidentType && g.Emitter == emitter).Select(g => g.Count).FirstOrDefault()
                }).ToList()
            }).ToList();

            var result = new List<IncidentsIntervalStatisticsDto>
            {
                new IncidentsIntervalStatisticsDto
                {
                    StatisticsType = StatisticsType.IncidentsByType,
                    Interval = interval,
                    Name = "Incidents by type",
                    IncidentsTypeStatisticsDtos = stats
                }
            };

            return new ResponseDto<IEnumerable<IncidentsIntervalStatisticsDto>>(result);
        }
    }
}