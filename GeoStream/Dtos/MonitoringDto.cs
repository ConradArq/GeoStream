using GeoStream.Dtos.Enums;
using static System.Net.Mime.MediaTypeNames;

namespace GeoStream.Dtos
{
    /// <summary>
    /// Represents a reading captured by the scanner, including details about the scanner, its location, and the asset associated with the Emitter, to be displayed on the monitoring page.
    /// This data is stored in a dedicated collection in MongoDB, and this model represents a document in that collection.
    /// </summary>
    public class MonitoringDto
    {
        /// <summary>
        /// Stored in MongoDB.
        /// </summary>
        public string Emitter { get; set; } = string.Empty;

        /// <summary>
        /// Stored in MongoDB.
        /// The date and time when the scanner read the Emitter.
        /// </summary>
        public DateTime DateTime { get; set; } = DateTime.Now;

        /// <summary>
        /// Stored in MongoDB.
        /// A unique code assigned to the scanner. 
        /// This code is defined in the appsettings.json of the Windows service application and must match the code provided to the scanner through the client app.
        /// </summary>
        public string ScannerCode { get; set; } = string.Empty;

        /// <summary>
        /// Stored in MongoDB.
        /// Helps keep a history of which scanners were installed at a specific hub at the time of the reading.
        /// </summary>
        public string HubCode { get; set; } = string.Empty;

        /// <summary>
        /// Stored in MongoDB.
        /// Used if the hub’s coordinates change, if hub data is deleted, or if the association between the scanner and the hub is lost.
        /// </summary>
        public decimal Latitude { get; set; }
        /// <summary>
        /// Stored in MongoDB.
        /// Used if the hub’s coordinates change, if hub data is deleted, or if the association between the scanner and the hub is lost.
        /// </summary>
        public decimal Longitude { get; set; }
        /// <summary>
        /// Stored in MongoDB.
        /// Used if the hub’s coordinates change, if hub data is deleted, or if the association between the scanner and the hub is lost.
        /// </summary>
        public float LaneDirectionDegrees { get; set; }
        /// <summary>
        /// Stored in MongoDB.
        /// Used if the hub’s coordinates change, if hub data is deleted, or if the association between the scanner and the hub is lost.
        /// </summary>
        public string Destination { get; set; } = string.Empty;

        /// <summary>
        /// Stored in MongoDB.
        /// The asset code is stored in MongoDB to maintain a history of which Emitters are associated with which asset codes. 
        /// If, at the time the scanner reads the emitter, the emitter was associated with a different asset code than the current one, 
        /// the client app will show the asset code that was registered at the time of the reading and indicate that the asset code has since changed.
        /// </summary>
        public string AssetCode { get; set; } = string.Empty;

        /// <summary>
        /// Used for display
        /// </summary>
        public int HubId { get; set; }

        /// <summary>
        /// Used for display
        /// </summary>
        public string HubName { get; set; } = string.Empty;

        /// <summary>
        /// Used for display (ScannerCode + Destination)
        /// </summary>
        public string ScannerCodeDestination { get; set; } = string.Empty;

        /// <summary>
        /// Used for navigation to asset page in JS leaflet map
        /// </summary>
        public int AssetId { get; set; }  
    }
}
