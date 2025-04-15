using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using GeoStream.Api.Domain.Enums;

namespace GeoStream.Api.Application.Dtos.Emitter
{
    //Instructs BSON serializer to ignore fields present in DB but not in this model. Necessary for backwards compatibility because some fields are no longer stored in MongoDB.
    [BsonIgnoreExtraElements]
    /// <summary>
    /// Represents a reading captured by the scanner, including details about the scanner, its location, and the asset associated with the Emitter.
    /// This data is stored in a dedicated collection in MongoDB, and this model represents a document in that collection.
    /// </summary>
    public class ResponseEmitterDto
    {
        public ObjectId Id { get; set; }
        public string Emitter { get; set; } = string.Empty;
        public DateTime DateTime { get; set; } = DateTime.Now;
        public string ScannerCode { get; set; } = string.Empty;
        public string HubCode { get; set; } = string.Empty;      
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public float LaneDirectionDegrees { get; set; }
        public string Destination { get; set; } = string.Empty;     
        public string AssetCode { get; set; } = string.Empty;
    }

    public class ResponseIncidentEmitterDto : ResponseEmitterDto
    {
        public IncidentType IncidentType { get; set; }
    }
}
