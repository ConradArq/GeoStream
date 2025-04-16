using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using GeoStream.Api.Application.Dtos;
using GeoStream.Api.Application.Dtos.Asset;
using GeoStream.Api.Application.Exceptions;
using GeoStream.Api.Application.Interfaces.Services;
using GeoStream.Api.Domain.Enums;
using GeoStream.Api.Domain.Interfaces.Repositories;
using GeoStream.Api.Domain.Models.Entities;
using GeoStream.Api.Helpers;
using GeoStream.Api.Application.Strategies;
using Azure.Core;

namespace GeoStream.Api.Application.Services
{
    internal class AssetService : IAssetService
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;

        public AssetService(IMapper mapper, IUnitOfWork unitOfWork)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }

        public async Task<ResponseDto<ResponseAssetDto>> CreateAsync(CreateAssetDto requestDto)
        {
            // Check for existing Emitters
            var existingEmitters = await _unitOfWork.AssetEmitterRepository
                .GetAsync(vht => vht.EmitterCode == requestDto.Emitter);

            if (existingEmitters.Any())
                throw new ValidationException("A emitter with the same code already exists.");

            var entity = _mapper.Map<Asset>(requestDto);

            entity.AssetEmitters = new List<AssetEmitter>()
            {
                new AssetEmitter()
                {
                    EmitterCode = requestDto.Emitter
                }
            };

            _unitOfWork.AssetRepository.Create(entity);
            await _unitOfWork.SaveAsync();

            var response = new ResponseDto<ResponseAssetDto>(_mapper.Map<ResponseAssetDto>(entity));

            return response;
        }

        public async Task<ResponseDto<ResponseAssetDto>> UpdateAsync(int id, UpdateAssetDto requestDto)
        {
            var entity = await _unitOfWork.AssetRepository.GetSingleAsync(id, includes: Includes<Asset>(q => q.Include(x => x.AssetEmitters), q => q.Include(x => x.SpecialAccesss)));

            if (entity == null)
            {
                throw new NotFoundException(id);
            }

            // Check for existing Emitters
            var existingEmitters = await _unitOfWork.AssetEmitterRepository
                .GetAsync(vht => vht.EmitterCode == requestDto.Emitter && vht.AssetId != requestDto.Id);

            if (existingEmitters.Any())
                throw new ValidationException("A emitter with the same code already exists.");

            _mapper.Map(requestDto, entity);

            if (!string.IsNullOrEmpty(requestDto.Emitter) && !entity.AssetEmitters.Select(x=>x.EmitterCode).Contains(requestDto.Emitter))
            {
                foreach (var item in entity.AssetEmitters)
                {
                    item.StatusId = (int)Status.Inactive;
                }

                entity.AssetEmitters.Add(new AssetEmitter
                {
                    EmitterCode = requestDto.Emitter
                });
            }

            var syncSpecialAccessStrategy = new DefaultRelatedEntitySyncStrategy<Asset, SpecialAccess>(
                v => v.SpecialAccesss,
                v => _mapper.Map<List<SpecialAccess>>(requestDto.SpecialAccesss),
                HardDeleteStrategy<SpecialAccess>.Instance
            );

            syncSpecialAccessStrategy.Sync(entity, _unitOfWork);

            await _unitOfWork.SaveAsync();

            var response = new ResponseDto<ResponseAssetDto>(_mapper.Map<ResponseAssetDto>(entity));

            return response;
        }

        public async Task<ResponseDto<object>> DeleteAsync(int id)
        {
            var entity = await _unitOfWork.AssetRepository.GetSingleAsync(id, includes: Includes<Asset>(x=>x.Include(x=>x.SpecialAccesss), x => x.Include(x => x.AssetEmitters)));

            if (entity == null)
            {
                throw new NotFoundException(id);
            }

            _unitOfWork.AssetRepository.Delete(entity);
            await _unitOfWork.SaveAsync();

            var response = new ResponseDto<object>();
            return response;
        }

        public async Task<ResponseDto<ResponseAssetDto>> GetAsync(int id)
        {
            var selector = new Func<IQueryable<Asset>, IQueryable<ResponseAssetDto>>(query => query
                .ProjectTo<ResponseAssetDto>(_mapper.ConfigurationProvider)
            );
            var responseDto = await _unitOfWork.AssetRepository.GetSingleAsync(id,
                selector: selector
            );

            if (responseDto == null)
            {
                throw new NotFoundException(id);
            }

            var response = new ResponseDto<ResponseAssetDto>(responseDto);
            return response;
        }

        public async Task<ResponseDto<IEnumerable<ResponseAssetDto>>> GetAllAsync(RequestDto? requestDto)
        {
            var selector = new Func<IQueryable<Asset>, IQueryable<ResponseAssetDto>>(query => query
                .ProjectTo<ResponseAssetDto>(_mapper.ConfigurationProvider)
            );

            var responseDtos = await _unitOfWork.AssetRepository.GetAsync(
                orderBy: BuildOrderByFunction<Asset>(requestDto),
                selector: selector
            );

            var response = new ResponseDto<IEnumerable<ResponseAssetDto>>(responseDtos);
            return response;
        }

        public async Task<PaginatedResponseDto<IEnumerable<ResponseAssetDto>>> GetAllPaginatedAsync(PaginationRequestDto requestDto)
        {
            var entities = await _unitOfWork.AssetRepository.GetPaginatedAsync(requestDto.PageNumber, requestDto.PageSize, orderBy: BuildOrderByFunction<Asset>(requestDto));

            var response = new PaginatedResponseDto<IEnumerable<ResponseAssetDto>>(_mapper.Map<IEnumerable<ResponseAssetDto>>(entities.Data), requestDto.PageNumber, requestDto.PageSize, entities.TotalItems);
            return response;
        }

        public async Task<ResponseDto<IEnumerable<ResponseAssetDto>>> SearchAsync(SearchAssetDto requestDto)
        {
            var searchExpression = BuildPredicate<Asset>(requestDto);
            var entities = await _unitOfWork.AssetRepository.GetAsync(searchExpression, orderBy: BuildOrderByFunction<Asset>(requestDto));

            var response = new ResponseDto<IEnumerable<ResponseAssetDto>>(_mapper.Map<IEnumerable<ResponseAssetDto>>(entities));
            return response;
        }

        public async Task<PaginatedResponseDto<IEnumerable<ResponseAssetDto>>> SearchPaginatedAsync(SearchPaginatedAssetDto requestDto)
        {
            var searchExpression = BuildPredicate<Asset>(requestDto);
            var entities = await _unitOfWork.AssetRepository.GetPaginatedAsync(requestDto.PageNumber, requestDto.PageSize, searchExpression, orderBy: BuildOrderByFunction<Asset>(requestDto));

            var response = new PaginatedResponseDto<IEnumerable<ResponseAssetDto>>(_mapper.Map<IEnumerable<ResponseAssetDto>>(entities.Data), requestDto.PageNumber, requestDto.PageSize, entities.TotalItems);
            return response;
        }
    }
}
