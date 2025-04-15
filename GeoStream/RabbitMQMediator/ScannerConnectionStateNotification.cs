namespace GeoStream.RabbitMQMediator
{
    using MediatR;
    using GeoStream.RabbitMQ.Models;

    public class ScannerConnectionStateNotification : INotification
    {
        public ScannerConnectionStateLog ScannerConnectionStateLog { get; set; }

        public ScannerConnectionStateNotification(ScannerConnectionStateLog ScannerConnectionStateLog)
        {
            this.ScannerConnectionStateLog = ScannerConnectionStateLog;
        }
    }
}
