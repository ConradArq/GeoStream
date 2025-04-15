using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoStream.Api.Application.Dtos.Scanner
{
    public class SearchPaginatedScannerDto : PaginationRequestDto
    {
        public string? Code { get; set; }
        public float? LaneDirectionDegrees { get; set; }
        public string? LaneDestination { get; set; }
        public bool? IsConnected { get; set; }
        public int? EmitterReadingIntervalInMinutes { get; set; }
        public int? HubId { get; set; }
    }
}
