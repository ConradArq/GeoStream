using System.ComponentModel.DataAnnotations;

namespace GeoStream.Dtos.Enums
{
    public enum Reports
    {
        [Display(Name = "Scanner Activity Report")]
        ScannerActivityReport = 1,
        [Display(Name = "Asset Activity Report")]
        AssetActivityReport = 2,
    }
}
