using System.ComponentModel.DataAnnotations;

namespace GeoStream.Dtos.Enums
{
    public enum IncidentType
    {
        [Display(Name = "General")]
        General = 0,

        [Display(Name = "Asset stopped too long in a restricted area")]
        UnexpectedStop = 1,

        [Display(Name = "Excessive dwell time at a hub")]
        IdleTooLong = 2,

        [Display(Name = "Asset left earlier than scheduled")]
        EarlyDeparture = 3,
    }
}
