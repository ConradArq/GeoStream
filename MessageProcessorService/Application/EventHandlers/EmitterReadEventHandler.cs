using MessageProcessorService.Application.Commands;
using MessageProcessorService.Core.Interfaces;
using MessageProcessorService.Domain.Enums;
using MessageProcessorService.Domain.Events;
using MessageProcessorService.Domain.Interfaces;
using MessageProcessorService.Domain.Models;

namespace MessageProcessorService.Application.EventHandlers
{
    public class EmitterReadEventHandler : IEventHandler<EmitterReadEvent>
    {
        private static SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);
        private readonly IHttpClientFactory _clientFactory;
        private readonly IEmitterRepository _emitterRepository;
        private readonly IGeoStreamRepository _geoStreamRepository;
        private readonly IEventBus _bus;

        public EmitterReadEventHandler(IHttpClientFactory clientFactory, IEmitterRepository emitterRepository, IGeoStreamRepository geoStreamRepository, IEventBus bus)
        {
            _clientFactory = clientFactory;
            _emitterRepository = emitterRepository;
            _geoStreamRepository = geoStreamRepository;
            _bus = bus;
        }

        /// <summary>
        /// Triggers when a emitter is read from rabbitmq. 
        /// Populates EmitterLog object with info about the asset and hubs to be inserted in MongoDB at a later stage.
        /// Identify incidents occurred to the asset and populates array with incidents types which will be inserted 
        /// in MongoDB at a later stage.
        /// </summary>
        /// <param name="event">The EmitterCreatedEvent event (ScannerCode and Emitter) pulled from RabbitMQ</param>
        /// <returns>Void</returns>

        public async Task Handle(EmitterReadEvent @event)
        {
            //Wait until emitter has been saved in mongodb and command has been sent to publish event in rabbitMQ
            await semaphoreSlim.WaitAsync();
            try
            {
                var scanner = _geoStreamRepository.GetScannerBy(@event.ScannerCode);

                // Only adds emitters to MongoDB if no matching emitters are found in the last 'minutesThreshold'. This prevents inserting
                // repeated emitters, as scanners can read the same emitter multiple times in a very short timeframe. If 'minutesThreshold'
                // is passed as null, the value is pulled from appsettings.json.
                if (_emitterRepository.IsEmitterInsertedSince(@event.EmitterCode, scanner?.EmitterReadingIntervalInMinutes))
                {
                    return;
                }

                var emitterLog = new EmitterLog()
                {
                    Code = @event.EmitterCode,
                    DateTime = @event.ReadTimestamp,
                    ScannerCode = @event.ScannerCode
                };

                var hub = _geoStreamRepository.GetHubBy(@event.ScannerCode);
                var assetIdAndCode = _geoStreamRepository.GetAssetIdAndCodeBy(@event.EmitterCode);

                emitterLog.HubCode = hub?.Code ?? string.Empty;
                emitterLog.Latitude = hub?.Latitude ?? 0;
                emitterLog.Longitude = hub?.Longitude ?? 0;
                emitterLog.LaneDirectionDegrees = scanner?.LaneDirectionAngle ?? 0;
                emitterLog.Destination = scanner?.Destination ?? string.Empty;
                emitterLog.AssetCode = assetIdAndCode?.Item2 ?? string.Empty;

                #region Incidents

                List<IncidentType> incidentTypes = new List<IncidentType>();

                if (!string.IsNullOrEmpty(emitterLog.AssetCode))
                {
                    if (!_emitterRepository.IsEmitterIncidentInsertedSince(@event.EmitterCode, IncidentType.UnexpectedStop, null))
                    {
                        // TODO: Use _clientFactory to call API to check if the asset has stopped unexpectedly and add incident if it has.

                        incidentTypes.Add(IncidentType.UnexpectedStop);
                    }

                    if (!_emitterRepository.IsEmitterIncidentInsertedSince(@event.EmitterCode, IncidentType.EarlyDeparture, null))
                    {
                        // TODO: Use _clientFactory call API to check if the asset has departed earlier than expected and add incident if it has.

                        incidentTypes.Add(IncidentType.EarlyDeparture);
                    }

                    if (!_emitterRepository.IsEmitterIncidentInsertedSince(@event.EmitterCode, IncidentType.IdleTooLong, null))
                    {
                        // TODO: Use _clientFactory call API to check if the asset has spent too much time at the hub and add incident if it has.

                        incidentTypes.Add(IncidentType.IdleTooLong);
                    }
                }

                #endregion

                // Insert in MongoDB
                _emitterRepository.InsertEmitterLog(emitterLog, incidentTypes);

                EmitterStoredCommand emitterStoredCommand = new()
                {
                    Code = emitterLog.Code,
                    ReadTimestamp = emitterLog.DateTime,
                    ScannerCode = emitterLog.ScannerCode,
                    HubCode = emitterLog.HubCode,
                    Latitude = emitterLog.Latitude,
                    Longitude = emitterLog.Longitude,
                    LaneDirectionDegrees = emitterLog.LaneDirectionDegrees,
                    Destination = emitterLog.Destination,
                    AssetCode = emitterLog.AssetCode,
                    IncidentTypes = incidentTypes
                };

                // Send command to publish in RabbitMQ
                await _bus.SendCommand(emitterStoredCommand);
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }
    }
}
