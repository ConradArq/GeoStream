using GeoStream.Dtos.Enums;

namespace GeoStream.Dtos
{
    /// <summary>
    /// Represents an incident occurred to an asset associated with an Emitter read by the scanner, to be displayed on the map as an incident.
    /// When an Emitter signal is captured by the scanner, the system queries the AssetRegistry APIs to determine if any incidents occurred while passing by the scanner. 
    /// Incidents are stored in a dedicated collection in MongoDB, and this model represents a document in that collection.
    /// It also includes details about the scanner, its location, and the asset associated with the Emitter.
    /// </summary>
    public class IncidentDto: MonitoringDto
    {
        /// <summary>
        /// Stored in MongoDB.
        /// Type of incident occurred to the asset. 
        /// </summary>
        public IncidentType IncidentType { get; set; }
    }
}
