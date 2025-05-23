﻿namespace GeoStream.Api.Application.Dtos.Location
{
    public class UpdateLocationDto
    {
        public int Id { get; set; }
        public int? CountryId { get; set; }
        public int? RegionId { get; set; }
        public int? DistrictId { get; set; }
        public int StatusId { get; set; }
    }
}
