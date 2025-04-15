using MessageProcessorService.Domain.Enums;
using MessageProcessorService.Domain.Interfaces;
using MessageProcessorService.Domain.Models;
using MongoDB.Driver;

namespace MessageProcessorService.Infrastructure.Persistence.MongoDB.Respositories
{
    public class EmitterRepository : IEmitterRepository
    {
        private readonly MongoDBSettings _settings;

        public EmitterRepository(MongoDBSettings settings)
        {
            _settings = settings;
        }


        public void InsertEmitterLog(EmitterLog emitterLog, List<IncidentType> incidentTypes)
        {
            var client = new MongoClient(_settings.Connection);
            var collection = client.GetDatabase(_settings.Database).GetCollection<EmitterLog>(_settings.EmitterCollection);

            collection.InsertOne(emitterLog);

            if (incidentTypes.Count() > 0)
            {
                var collectionIncidents = client.GetDatabase(_settings.Database).GetCollection<IncidentEmitterLog>(_settings.IncidentCollection);

                foreach (var incidentType in incidentTypes)
                {
                    collectionIncidents.InsertOne(new IncidentEmitterLog()
                    {
                        IncidentType = incidentType,
                        Code = emitterLog.Code,
                        DateTime = emitterLog.DateTime,
                        ScannerCode = emitterLog.ScannerCode,
                        HubCode = emitterLog.HubCode,
                        Latitude = emitterLog.Latitude,
                        Longitude = emitterLog.Longitude,
                        LaneDirectionDegrees = emitterLog.LaneDirectionDegrees,
                        Destination = emitterLog.Destination,
                        AssetCode = emitterLog.AssetCode
                    });
                }
            }
        }

        public bool IsEmitterInsertedSince(string emitterCode, int? minutesThreshold)
        {
            var client = new MongoClient(_settings.Connection);
            var collection = client.GetDatabase(_settings.Database).GetCollection<EmitterLog>(_settings.EmitterCollection);

            var filter = Builders<EmitterLog>.Filter.Eq(r => r.Code, emitterCode)
                         & Builders<EmitterLog>.Filter.Gte(x => x.DateTime, DateTime.Now.AddMinutes(-(minutesThreshold ?? _settings.MinutesThreshold)));

            var emitters = collection.Find(filter);

            if (emitters.CountDocuments() > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool IsEmitterIncidentInsertedSince(string emitterCode, IncidentType incidentType, int? minutesThreshold)
        {
            var client = new MongoClient(_settings.Connection);
            var collection = client.GetDatabase(_settings.Database).GetCollection<IncidentEmitterLog>(_settings.IncidentCollection);

            var filter = Builders<IncidentEmitterLog>.Filter.Eq(r => r.Code, emitterCode)
                         & Builders<IncidentEmitterLog>.Filter.Eq(x => x.IncidentType, incidentType)
                         & Builders<IncidentEmitterLog>.Filter.Gte(x => x.DateTime, DateTime.Now.AddMinutes(-(minutesThreshold ?? _settings.IncidentMinutesThreshold)).ToUniversalTime());

            var emitters = collection.Find(filter);

            if (emitters.CountDocuments() > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }        
    }
}