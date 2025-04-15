using MessageProcessorService.Core.Commands;
using MessageProcessorService.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageProcessorService.Application.Commands
{
    public class EmitterStoredCommand : EmitterCommand
    {
        public string ScannerCode { get; set; } = string.Empty;
        public string HubCode { get; set; } = string.Empty;
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public float LaneDirectionDegrees { get; set; }
        public string Destination { get; set; } = string.Empty;
        public string AssetCode { get; set; } = string.Empty;
        public List<IncidentType> IncidentTypes { get; set; } = new List<IncidentType>();
    }
}
