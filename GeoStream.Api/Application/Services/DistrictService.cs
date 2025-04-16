using AutoMapper;
using GeoStream.Api.Application.Dtos;
using GeoStream.Api.Application.Dtos.District;
using GeoStream.Api.Application.Interfaces.Services;
using GeoStream.Api.Domain.Interfaces.Repositories;
using GeoStream.Api.Domain.Models.Entities;
using GeoStream.Api.Helpers;

namespace GeoStream.Api.Application.Services
{
    internal class DistrictService : IDistrictService
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;

        public DistrictService(IMapper mapper, IUnitOfWork unitOfWork)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }

        public async Task<ResponseDto<IEnumerable<ResponseDistrictDto>>> SearchAsync(SearchDistrictDto requestDto)
        {
            var searchExpression = BuildPredicate<District>(requestDto);

            var entities = await _unitOfWork.DistrictRepository.GetAsync(
                predicate: searchExpression,
                orderBy: BuildOrderByFunction<District>(requestDto)
            );

            var response = new ResponseDto<IEnumerable<ResponseDistrictDto>>(_mapper.Map<IEnumerable<ResponseDistrictDto>>(entities));
            return response;
        }
    }
}
