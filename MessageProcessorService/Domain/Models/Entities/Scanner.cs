﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageProcessorService.Domain.Models.Entities
{
    public class Scanner
    {
        public string Code { get; set; } = string.Empty;
        public int HubId { get; set; }
        public float LaneDirectionAngle { get; set; }
        public string Destination { get; set; } = string.Empty;
        public int? EmitterReadingIntervalInMinutes { get; set; }
    }
}
