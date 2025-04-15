using AutoMapper;
using GeoStream.Api.Application.Dtos;
using GeoStream.Api.Application.Dtos.Region;
using GeoStream.Api.Application.Interfaces.Services;
using GeoStream.Api.Domain.Interfaces.Repositories;
using GeoStream.Api.Domain.Models.Entities;
using GeoStream.Api.Helpers;

namespace GeoStream.Api.Application.Services
{
    internal class RegionService : IRegionService
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;

        public RegionService(IMapper mapper, IUnitOfWork unitOfWork)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }

        public async Task<ResponseDto<IEnumerable<ResponseRegionDto>>> SearchAsync(SearchRegionDto requestDto)
        {
            var searchExpression = QueryHelper.BuildPredicate<Region>(requestDto);

            var entities = await _unitOfWork.RegionRepository.GetAsync(
                predicate: searchExpression,
                orderBy: QueryHelper.BuildOrderByFunction<Region>(requestDto)
            );

            var response = new ResponseDto<IEnumerable<ResponseRegionDto>>(_mapper.Map<IEnumerable<ResponseRegionDto>>(entities));
            return response;
        }
    }
}
