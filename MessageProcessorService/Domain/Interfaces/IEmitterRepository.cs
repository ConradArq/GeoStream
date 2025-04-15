using MessageProcessorService.Domain.Enums;
using MessageProcessorService.Domain.Models;

namespace MessageProcessorService.Domain.Interfaces
{
    public interface IEmitterRepository
    {
        void InsertEmitterLog(EmitterLog emitterLog, List<IncidentType> incidentTypes);
        bool IsEmitterInsertedSince(string emitterCode, int? minutesThreshold);
        bool IsEmitterIncidentInsertedSince(string emitterCode, IncidentType incidentType, int? minutesThreshold);
    }
}
