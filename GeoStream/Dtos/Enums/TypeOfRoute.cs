using System.ComponentModel.DataAnnotations;

namespace GeoStream.Dtos.Enums
{
    public enum TypeOfRoute
    {
        [Display(Name = "Urban")]
        Urban = 1,

        [Display(Name = "Interurban")]
        Interurban = 2
    }
}
