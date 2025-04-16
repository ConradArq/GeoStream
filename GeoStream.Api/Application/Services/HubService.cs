using AutoMapper;
using GeoStream.Api.Application.Dtos;
using GeoStream.Api.Application.Dtos.Hub;
using GeoStream.Api.Application.Exceptions;
using GeoStream.Api.Application.Interfaces.Services;
using GeoStream.Api.Domain.Interfaces.Repositories;
using GeoStream.Api.Domain.Models.Entities;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using GeoStream.Api.Helpers;

namespace GeoStream.Api.Application.Services
{
    internal class HubService : IHubService
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;

        public HubService(IMapper mapper, IUnitOfWork unitOfWork)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }

        public async Task<ResponseDto<ResponseHubDto>> CreateAsync(CreateHubDto requestDto)
        {
            // Check for existing hubs with the same Name or Code
            var existingHubs = await _unitOfWork.HubRepository
                .GetAsync(s => s.Name == requestDto.Name || s.Code == requestDto.Code);

            if (existingHubs.Any())
                throw new ValidationException("A hub with the same name or code already exists.");

            // Check for duplicate scanner codes within the request
            var duplicateScannerCodes = requestDto.Scanners
                .GroupBy(a => a.Code)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicateScannerCodes.Any())
                throw new ValidationException($"Duplicate scanner codes found: {string.Join(", ", duplicateScannerCodes)}");

            // Check for existing scanners in the database with the same codes
            var existingScannerCodes = await _unitOfWork.ScannerRepository
                .GetAsync(a => requestDto.Scanners.Select(sa => sa.Code).Contains(a.Code));

            if (existingScannerCodes.Any())
                throw new ValidationException($"Scanner codes already exist: {string.Join(", ", existingScannerCodes.Select(a => a.Code))}");

            // Associate existing location or create a new one
            var existingLocation = (await _unitOfWork.LocationRepository.GetAsync(
                x =>
                    x.CountryId == requestDto.Location.CountryId
                    && x.RegionId == requestDto.Location.RegionId
                    && x.DistrictId == requestDto.Location.DistrictId,
                disableTracking: false
            )).FirstOrDefault();

            var entity = _mapper.Map<Hub>(requestDto);

            entity.Location = existingLocation ?? new Location()
            {
                CountryId = requestDto.Location.CountryId,
                RegionId = requestDto.Location.RegionId,
                DistrictId = requestDto.Location.DistrictId,
            };

            _unitOfWork.HubRepository.Create(entity);
            await _unitOfWork.SaveAsync();

            return new ResponseDto<ResponseHubDto>(_mapper.Map<ResponseHubDto>(entity));
        }

        public async Task<ResponseDto<ResponseHubDto>> UpdateAsync(int id, UpdateHubDto requestDto)
        {
            var entity = await _unitOfWork.HubRepository.GetSingleAsync(id, includes: Includes<Hub>(q => q.Include(x => x.Scanners)));

            if (entity == null)
                throw new NotFoundException(id);

            // Check for existing hubs with the same Name or Code, excluding the current one
            var existingHubs = await _unitOfWork.HubRepository
                .GetAsync(s => s.Id != id && (s.Name == requestDto.Name || s.Code == requestDto.Code));

            if (existingHubs.Any())
                throw new ValidationException("Another hub with the same name or code already exists.");

            // Check for duplicate scanner codes within the request
            var duplicateScannerCodes = requestDto.Scanners
                .GroupBy(a => a.Code)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicateScannerCodes.Any())
                throw new ValidationException($"Duplicate scanner codes found: {string.Join(", ", duplicateScannerCodes)}");

            // Check for existing scanners in the database with the same codes, excluding the current hub's scanners
            var existingScannerCodes = await _unitOfWork.ScannerRepository
                .GetAsync(a => a.HubId != id && requestDto.Scanners.Select(sa => sa.Code).Contains(a.Code));

            if (existingScannerCodes.Any())
                throw new ValidationException($"Scanner codes already exist: {string.Join(", ", existingScannerCodes.Select(a => a.Code))}");

            // Associate existing location or create a new one
            var existingLocation = (await _unitOfWork.LocationRepository.GetAsync(
                x =>
                    x.CountryId == requestDto.Location.CountryId
                    && x.RegionId == requestDto.Location.RegionId
                    && x.DistrictId == requestDto.Location.DistrictId,
                disableTracking: false
            )).FirstOrDefault();

            _mapper.Map(requestDto, entity);

            entity.Location = existingLocation ?? new Location()
            {
                CountryId = requestDto.Location.CountryId,
                RegionId = requestDto.Location.RegionId,
                DistrictId = requestDto.Location.DistrictId,
            };

            await _unitOfWork.SaveAsync();

            return new ResponseDto<ResponseHubDto>(_mapper.Map<ResponseHubDto>(entity));
        }

        public async Task<ResponseDto<object>> DeleteAsync(int id)
        {
            var entity = await _unitOfWork.HubRepository.GetSingleAsync(id);

            if (entity == null)
            {
                throw new NotFoundException(id);
            }

            _unitOfWork.HubRepository.Delete(entity);
            await _unitOfWork.SaveAsync();

            var response = new ResponseDto<object>();
            return response;
        }

        public async Task<ResponseDto<ResponseHubDto>> GetAsync(int id)
        {
            var selector = new Func<IQueryable<Hub>, IQueryable<ResponseHubDto>>(query => query
                .ProjectTo<ResponseHubDto>(_mapper.ConfigurationProvider)
            );
            var responseDto = await _unitOfWork.HubRepository.GetSingleAsync(id,
                selector: selector
            );

            if (responseDto == null)
            {
                throw new NotFoundException(id);
            }

            var response = new ResponseDto<ResponseHubDto>(responseDto);
            return response;
        }

        public async Task<ResponseDto<IEnumerable<ResponseHubDto>>> GetAllAsync(RequestDto? requestDto)
        {
            var selector = new Func<IQueryable<Hub>, IQueryable<ResponseHubDto>>(query => query
                .ProjectTo<ResponseHubDto>(_mapper.ConfigurationProvider)
            );

            var responseDtos = await _unitOfWork.HubRepository.GetAsync(
                orderBy: BuildOrderByFunction<Hub>(requestDto),
                selector: selector
            );

            var response = new ResponseDto<IEnumerable<ResponseHubDto>>(responseDtos);
            return response;
        }

        public async Task<PaginatedResponseDto<IEnumerable<ResponseHubDto>>> GetAllPaginatedAsync(PaginationRequestDto requestDto)
        {
            var entities = await _unitOfWork.HubRepository.GetPaginatedAsync(requestDto.PageNumber, requestDto.PageSize, orderBy: BuildOrderByFunction<Hub>(requestDto));

            var response = new PaginatedResponseDto<IEnumerable<ResponseHubDto>>(_mapper.Map<IEnumerable<ResponseHubDto>>(entities.Data), requestDto.PageNumber, requestDto.PageSize, entities.TotalItems);
            return response;
        }

        public async Task<ResponseDto<IEnumerable<ResponseHubDto>>> SearchAsync(SearchHubDto requestDto)
        {
            var searchExpression = BuildPredicate<Hub>(requestDto);

            var selector = new Func<IQueryable<Hub>, IQueryable<ResponseHubDto>>(query => query
                .ProjectTo<ResponseHubDto>(_mapper.ConfigurationProvider)
            );

            var responseDtos = await _unitOfWork.HubRepository.GetAsync(
                predicate: searchExpression,
                orderBy: BuildOrderByFunction<Hub>(requestDto),
                selector: selector
            );

            var response = new ResponseDto<IEnumerable<ResponseHubDto>>(responseDtos);
            return response;
        }

        public async Task<PaginatedResponseDto<IEnumerable<ResponseHubDto>>> SearchPaginatedAsync(SearchPaginatedHubDto requestDto)
        {
            var searchExpression = BuildPredicate<Hub>(requestDto);
            var entities = await _unitOfWork.HubRepository.GetPaginatedAsync(requestDto.PageNumber, requestDto.PageSize, searchExpression, orderBy: BuildOrderByFunction<Hub>(requestDto));

            var response = new PaginatedResponseDto<IEnumerable<ResponseHubDto>>(_mapper.Map<IEnumerable<ResponseHubDto>>(entities.Data), requestDto.PageNumber, requestDto.PageSize, entities.TotalItems);
            return response;
        }
    }
}
