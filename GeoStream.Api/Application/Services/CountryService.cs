using AutoMapper;
using GeoStream.Api.Application.Dtos;
using GeoStream.Api.Application.Dtos.Country;
using GeoStream.Api.Application.Interfaces.Services;
using GeoStream.Api.Domain.Interfaces.Repositories;
using GeoStream.Api.Domain.Models.Entities;
using GeoStream.Api.Helpers;

namespace GeoStream.Api.Application.Services
{
    internal class CountryService : ICountryService
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;

        public CountryService(IMapper mapper, IUnitOfWork unitOfWork)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }

        public async Task<ResponseDto<IEnumerable<ResponseCountryDto>>> SearchAsync(SearchCountryDto requestDto)
        {
            var searchExpression = BuildPredicate<Country>(requestDto);

            var entities = await _unitOfWork.CountryRepository.GetAsync(
                predicate: searchExpression,
                orderBy: BuildOrderByFunction<Country>(requestDto)
            );

            var response = new ResponseDto<IEnumerable<ResponseCountryDto>>(_mapper.Map<IEnumerable<ResponseCountryDto>>(entities));
            return response;
        }
    }
}
