using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeoStream.Api.Application.Dtos.SpecialAccess;

namespace GeoStream.Api.Application.Dtos.Asset
{
    public class SearchPaginatedAssetDto : PaginationRequestDto
    {
        public int? Id { get; set; }
        public string? Code { get; set; }
        public string? Emitter { get; set; }
        public string? OwnerDocumentNumber { get; set; }
        public int? RouteId { get; set; }
    }
}
