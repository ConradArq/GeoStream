using GeoStream.Models.Enums;

namespace GeoStream.Dtos
{
    public class BaseDto
    {
        public int StatusId { get; set; } = (int)Status.Active;

        /// <summary>
        /// Checks if the current status is Active.
        /// </summary>
        public bool IsActive => StatusId == (int)Status.Active;

        /// <summary>
        /// Switches the status between Active and Inactive.
        /// </summary>
        public void SwitchStatus()
        {
            StatusId = IsActive ? (int)Status.Inactive : (int)Status.Active;
        }
    }
}
