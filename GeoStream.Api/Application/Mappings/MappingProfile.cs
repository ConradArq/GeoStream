using AutoMapper;
using GeoStream.Api.Application.Dtos.Country;
using GeoStream.Api.Application.Dtos.District;
using GeoStream.Api.Application.Dtos.Location;
using GeoStream.Api.Application.Dtos.SpecialAccess;
using GeoStream.Api.Application.Dtos.Region;
using GeoStream.Api.Application.Dtos.Route;
using GeoStream.Api.Application.Dtos.RouteHub;
using GeoStream.Api.Application.Dtos.Scanner;
using GeoStream.Api.Application.Dtos.Hub;
using GeoStream.Api.Application.Dtos.Asset;
using GeoStream.Api.Domain.Models.Entities;
using Route = GeoStream.Api.Domain.Models.Entities.Route;

namespace GeoStream.Api.Application.Mappings
{
    internal class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // When using ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null)) in mappings, if src is a
            // nullable value type and dest isn't nullable, and src is passed as null, AutoMapper will convert it  to its corresponding
            // default value (e.g., 0 for int, false for bool) before mapping. This behavior bypasses the condition, causing unintended
            // overwrites of destination values. To prevent this, we explicitly map  nullable value types to their non-nullable
            // counterparts using ConvertUsing, ensuring that dest is left unaltered when src is null. 
            // For reference types, the condition works as expected, so no additional configuration is needed.
            CreateMap<int?, int>().ConvertUsing((src, dest) => src ?? dest);
            CreateMap<long?, long>().ConvertUsing((src, dest) => src ?? dest);
            CreateMap<short?, short>().ConvertUsing((src, dest) => src ?? dest);
            CreateMap<byte?, byte>().ConvertUsing((src, dest) => src ?? dest);
            CreateMap<float?, float>().ConvertUsing((src, dest) => src ?? dest);
            CreateMap<double?, double>().ConvertUsing((src, dest) => src ?? dest);
            CreateMap<decimal?, decimal>().ConvertUsing((src, dest) => src ?? dest);
            CreateMap<bool?, bool>().ConvertUsing((src, dest) => src ?? dest);
            CreateMap<char?, char>().ConvertUsing((src, dest) => src ?? dest);
            CreateMap<DateTime?, DateTime>().ConvertUsing((src, dest) => src ?? dest);
            CreateMap<Guid?, Guid>().ConvertUsing((src, dest) => src ?? dest);

            // Route
            CreateMap<CreateRouteDto, Route>();
            CreateMap<UpdateRouteDto, Route>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<Route, ResponseRouteDto>();

            // RouteHub
            CreateMap<RouteHub, ResponseRouteHubDto>();

            // Scanner
            CreateMap<CreateScannerDto, Scanner>();
            CreateMap<UpdateScannerDto, Scanner>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<Scanner, ResponseScannerDto>()
                .ForMember(dest => dest.HubName, opt => opt.MapFrom(src => src.Hub.Name))
                .ForMember(dest => dest.HubCode, opt => opt.MapFrom(src => src.Hub.Code))
                .ForMember(dest => dest.CountryName, opt => opt.MapFrom(src => src.Hub.Location.Country.Name))
                .ForMember(dest => dest.HubName, opt => opt.MapFrom(src => src.Hub.Location.Region.Name))
                .ForMember(dest => dest.HubName, opt => opt.MapFrom(src => src.Hub.Location.District.Name));

            // Hub
            CreateMap<CreateHubDto, Hub>()
                .ForMember(dest => dest.Location, opt => opt.Ignore());
            CreateMap<UpdateHubDto, Hub>()
                .ForMember(dest => dest.Location, opt => opt.Ignore())
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<Hub, ResponseHubDto>();

            // Asset
            CreateMap<CreateAssetDto, Asset>();
            CreateMap<UpdateAssetDto, Asset>()
                .ForMember(dest => dest.SpecialAccesss, opt => opt.Ignore())
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<Asset, ResponseAssetDto>()
                .ForMember(dest => dest.Emitter, opt => opt.MapFrom(src =>
                    src.AssetEmitters.FirstOrDefault(vht => vht.StatusId == (int)Domain.Enums.Status.Active) != null
                        ? src.AssetEmitters.First(vht => vht.StatusId == (int)Domain.Enums.Status.Active).EmitterCode
                        : null
                ));

            // SpecialAccess            
            CreateMap<CreateSpecialAccessDto, SpecialAccess>();
            CreateMap<UpdateSpecialAccessDto, SpecialAccess>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<SpecialAccess, ResponseSpecialAccessDto>();

            // Location
            CreateMap<Location, ResponseLocationDto>()
                .ForMember(dest => dest.CountryName, opt => opt.MapFrom(src => src.Country.Name))
                .ForMember(dest => dest.RegionName, opt => opt.MapFrom(src => src.Region.Name))
                .ForMember(dest => dest.DistrictName, opt => opt.MapFrom(src => src.District.Name));

            // Country
            CreateMap<Country, ResponseCountryDto>();

            // Region
            CreateMap<Region, ResponseRegionDto>();

            // District
            CreateMap<District, ResponseDistrictDto>();
        }
    }
}
