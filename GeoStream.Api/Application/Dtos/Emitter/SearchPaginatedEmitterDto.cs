namespace GeoStream.Api.Application.Dtos.Emitter
{
    public class SearchPaginatedEmitterDto: PaginationRequestDto
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }

        public List<int>? IncidentTypes { get; set; }
        public List<string>? HubCodes { get; set; }
        public List<string>? AntennaCodes { get; set; }
        public List<string>? AssetCodes { get; set; }
        public List<string>? Emitter { get; set; }
        public string SearchString { get; set; } = string.Empty;
    }
}
