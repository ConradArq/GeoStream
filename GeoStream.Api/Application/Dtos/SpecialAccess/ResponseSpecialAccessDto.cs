﻿namespace GeoStream.Api.Application.Dtos.SpecialAccess
{
    public class ResponseSpecialAccessDto
    {
        public int Id { get; set; }
        public int AssetId { get; set; }
        public int RouteId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
