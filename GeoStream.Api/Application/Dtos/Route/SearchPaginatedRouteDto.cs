using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeoStream.Api.Domain.Enums;

namespace GeoStream.Api.Application.Dtos.Route
{
    public class SearchPaginatedRouteDto : PaginationRequestDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }

        public byte[]? KmzFile { get; set; }
        public string? GeoJson { get; set; }
        public string? Color { get; set; }
        public TypeOfRoute? TypeOfRoute { get; set; }
    }
}
