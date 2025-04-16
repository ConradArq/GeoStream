using AutoMapper;
using AutoMapper.QueryableExtensions;
using GeoStream.Api.Application.Dtos;
using GeoStream.Api.Application.Dtos.Scanner;
using GeoStream.Api.Application.Dtos.Hub;
using GeoStream.Api.Application.Exceptions;
using GeoStream.Api.Application.Interfaces.Services;
using GeoStream.Api.Domain.Interfaces.Repositories;
using GeoStream.Api.Domain.Models.Entities;
using GeoStream.Api.Helpers;

namespace GeoStream.Api.Application.Services
{
    internal class ScannerService : IScannerService
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;

        public ScannerService(IMapper mapper, IUnitOfWork unitOfWork)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }
        public async Task<ResponseDto<ResponseScannerDto>> CreateAsync(CreateScannerDto requestDto)
        {
            var entity = _mapper.Map<Scanner>(requestDto);

            _unitOfWork.ScannerRepository.Create(entity);
            await _unitOfWork.SaveAsync();

            var response = new ResponseDto<ResponseScannerDto>(_mapper.Map<ResponseScannerDto>(entity));
            return response;
        }

        public async Task<ResponseDto<ResponseScannerDto>> UpdateAsync(int id, UpdateScannerDto requestDto)
        {
            var entity = await _unitOfWork.ScannerRepository.GetSingleAsync(id);

            if (entity == null)
            {
                throw new NotFoundException(id);
            }

            _mapper.Map(requestDto, entity);
            await _unitOfWork.SaveAsync();

            var response = new ResponseDto<ResponseScannerDto>(_mapper.Map<ResponseScannerDto>(entity));
            return response;
        }

        public async Task<ResponseDto<object>> DeleteAsync(int id)
        {
            var entity = await _unitOfWork.ScannerRepository.GetSingleAsync(id);

            if (entity == null)
            {
                throw new NotFoundException(id);
            }

            _unitOfWork.ScannerRepository.Delete(entity);
            await _unitOfWork.SaveAsync();

            var response = new ResponseDto<object>();
            return response;
        }

        public async Task<ResponseDto<ResponseScannerDto>> GetAsync(int id)
        {
            var entity = await _unitOfWork.ScannerRepository.GetSingleAsync(id);

            if (entity == null)
            {
                throw new NotFoundException(id);
            }

            var response = new ResponseDto<ResponseScannerDto>(_mapper.Map<ResponseScannerDto>(entity));
            return response;
        }

        public async Task<ResponseDto<IEnumerable<ResponseScannerDto>>> GetAllAsync(RequestDto? requestDto)
        {
            var selector = new Func<IQueryable<Scanner>, IQueryable<ResponseScannerDto>>(query => query
                .ProjectTo<ResponseScannerDto>(_mapper.ConfigurationProvider)
            );

            var responseDtos = await _unitOfWork.ScannerRepository.GetAsync(
                orderBy: BuildOrderByFunction<Scanner>(requestDto),
                selector: selector
            );

            var response = new ResponseDto<IEnumerable<ResponseScannerDto>>(responseDtos);
            return response;
        }

        public async Task<PaginatedResponseDto<IEnumerable<ResponseScannerDto>>> GetAllPaginatedAsync(PaginationRequestDto requestDto)
        {
            var entities = await _unitOfWork.ScannerRepository.GetPaginatedAsync(requestDto.PageNumber, requestDto.PageSize, orderBy: BuildOrderByFunction<Scanner>(requestDto));

            var response = new PaginatedResponseDto<IEnumerable<ResponseScannerDto>>(_mapper.Map<IEnumerable<ResponseScannerDto>>(entities.Data), requestDto.PageNumber, requestDto.PageSize, entities.TotalItems);
            return response;
        }

        public async Task<ResponseDto<IEnumerable<ResponseScannerDto>>> SearchAsync(SearchScannerDto requestDto)
        {
            var searchExpression = BuildPredicate<Scanner>(requestDto);
            var entities = await _unitOfWork.ScannerRepository.GetAsync(searchExpression, orderBy: BuildOrderByFunction<Scanner>(requestDto));

            var response = new ResponseDto<IEnumerable<ResponseScannerDto>>(_mapper.Map<IEnumerable<ResponseScannerDto>>(entities));
            return response;
        }

        public async Task<PaginatedResponseDto<IEnumerable<ResponseScannerDto>>> SearchPaginatedAsync(SearchPaginatedScannerDto requestDto)
        {
            var searchExpression = BuildPredicate<Scanner>(requestDto);
            var entities = await _unitOfWork.ScannerRepository.GetPaginatedAsync(requestDto.PageNumber, requestDto.PageSize, searchExpression, orderBy: BuildOrderByFunction<Scanner>(requestDto));

            var response = new PaginatedResponseDto<IEnumerable<ResponseScannerDto>>(_mapper.Map<IEnumerable<ResponseScannerDto>>(entities.Data), requestDto.PageNumber, requestDto.PageSize, entities.TotalItems);
            return response;
        }
    }
}