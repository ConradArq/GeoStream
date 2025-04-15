using System.ComponentModel;

namespace GeoStream.Api.Application.Dtos.RouteHub
{
    public class CreateRouteHubDto
    {
        public int RouteId { get; set; }
        public int HubId { get; set; }
    }
}