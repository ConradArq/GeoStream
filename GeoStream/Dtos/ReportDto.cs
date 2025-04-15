using GeoStream.Dtos.Enums;

namespace GeoStream.Dtos
{
    public class ReportDTO
    {
        public Reports Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string SelectedAssetCode { get; set; } = string.Empty;
    }
}
