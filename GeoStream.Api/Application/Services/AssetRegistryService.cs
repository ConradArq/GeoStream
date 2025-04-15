using AutoMapper;
using GeoStream.Api.Application.Dtos;
using GeoStream.Api.Application.Dtos.AssetRegistry;
using GeoStream.Api.Application.Interfaces.Services;
using GeoStream.Api.Domain.Interfaces.Repositories;

namespace GeoStream.Api.Application.Services
{
    internal class AssetRegistryService : IAssetRegistryService
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;

        public AssetRegistryService(IMapper mapper, IUnitOfWork unitOfWork)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }

        public Task<ResponseDto<IEnumerable<ResponseAssetRegistryDto>>> SearchAsync(SearchAssetRegistryDto requestDto)
        {
            // TODO: Implement logic to call the asset registry API
            return Task.FromResult
            (
                new ResponseDto<IEnumerable<ResponseAssetRegistryDto>>
                (
                    new List<ResponseAssetRegistryDto>
                    {
                        new ResponseAssetRegistryDto { OwnerDocumentNumber = string.Concat(Enumerable.Range(0, 8).Select(_ => new Random().Next(0, 10))) }
                    }
                )
            );
        }
    }
}
