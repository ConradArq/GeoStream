﻿using ApexCharts;
using GeoStream.Dtos.Enums;

namespace GeoStream.Dtos
{
    public class IncidentsIntervalStatisticsDto
    {
        public StatisticsType StatisticsType { get; set; }
        public SeriesType SeriesType { get; set; }
        public Interval Interval { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<IncidentsTypeStatisticsDto> IncidentsTypeStatisticsDtos { get; set; } = new List<IncidentsTypeStatisticsDto>();
    }

    public enum StatisticsType
    {
        IncidentsByType,
        IncidentsByCountry
    }
    public enum Interval
    {
        Daily,
        Monthly,
        Yearly
    }

    public class IncidentsTypeStatisticsDto
    {
        public IncidentType IncidentType { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<IncidentIntervalStatisticsDto> IncidentIntervalStatisticsDtos { get; set; } = new List<IncidentIntervalStatisticsDto>();
    }

    public class IncidentIntervalStatisticsDto
    {
        public string Emitter { get; set;} = string.Empty;
        public int IncidentCount { get; set; }
    }
}
