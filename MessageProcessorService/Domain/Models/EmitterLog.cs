using MongoDB.Bson;

namespace MessageProcessorService.Domain.Models
{
    public class EmitterLog
    {
        /// <summary>
        /// Used in MongoDB
        /// </summary>
        public ObjectId Id { get; set; }

        /// <summary>
        /// The Emitter identifier.
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// The date and time when the scanner read the Emitter.
        /// @event.ReadTimestamp.Kind is set to Local and the .Net MongoDB driver will convert it to Utc so that dates are always stored in Utc.
        /// </summary>
        public DateTime DateTime { get; set; } = DateTime.Now;

        /// <summary>
        /// A unique code assigned to the scanner. 
        /// This code is defined in the appsettings.json of the Windows service application and must match the code provided to the scanner through the client app.
        /// </summary>
        public string ScannerCode { get; set; } = string.Empty;

        /// <summary>
        /// The hub code is stored in MongoDB to keep a history of which scanners were installed at a specific hub at the time of the reading.
        /// </summary>
        public string HubCode { get; set; } = string.Empty;

        /// <summary>
        /// The latitude, longitude, LaneDirectionDegrees and Destination are stored in MongoDB as a backup. 
        /// These values are useful if the hub’s coordinates change, if hub data is deleted, or if the association between the scanner and the hub is lost.</summary>
        /// </summary>
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public float LaneDirectionDegrees { get; set; }
        public string Destination { get; set; } = string.Empty;

        /// <summary>
        /// Stored in MongoDB.
        /// The asset code is stored in MongoDB to maintain a history of which Emitters are associated with which asset codes. 
        /// If, at the time the scanner reads the emitter, the emitter was associated with a different asset code than the current one, 
        /// the client app will show the asset code that was registered at the time of the reading and indicate that the asset code has since changed.
        /// </summary>
        public string AssetCode { get; set; } = string.Empty;
    }
}
