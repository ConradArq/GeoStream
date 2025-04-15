using GeoStream.RabbitMQ.Contracts.Bus;
using GeoStream.RabbitMQ.Events;
using GeoStream.RabbitMQ.Models;
using MediatR;
using GeoStream.RabbitMQMediator;
using GeoStream.Services;
using GeoStream.Dtos.Configuration;
using GeoStream.Dtos;

namespace GeoStream.RabbitMQ.Bus
{
    public class ScannerConnectionStateEventHandler : IEventHandler<ScannerConnectionStateEvent>
    {
        private readonly IApiClient _apiClient;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public ScannerConnectionStateEventHandler(IApiClient apiClient, IServiceScopeFactory serviceScopeFactory)
        {
            _apiClient = apiClient;
            _serviceScopeFactory = serviceScopeFactory;
        }
        public async Task Handle(ScannerConnectionStateEvent @event)
        {
            if (!@event.IsConnected)
            {
                var response = await _apiClient.SendRequest<ApiResponseDto<List<ScannerDto>>>("ScannersApi.Scanner.GetBy", null, new Dictionary<string, string> { { "scannerCode", @event.ScannerCode } });

                if (response.Succeeded)
                {
                    var scannerToUpdate = response.Data.FirstOrDefault();
                    if (scannerToUpdate != null)
                    {
                        scannerToUpdate.IsConnected = false;
                        var result = await _apiClient.SendRequest<ApiResponseDto<ScannerDto>>("ScannersApi.Scanner.AddUpdate", scannerToUpdate);
                    }
                }
            }

            var scannerConnectionState = new ScannerConnectionStateLog()
            {
                ReadTimestamp = @event.ReadTimestamp,
                ScannerCode = @event.ScannerCode,
                IsConnected = @event.IsConnected,
            };

            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                await mediator.Publish(new ScannerConnectionStateNotification(scannerConnectionState));
            }
        }
    }
}
