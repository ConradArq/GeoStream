namespace GeoStream.RabbitMQMediator
{
    using MediatR;
    using GeoStream.RabbitMQ.Models;

    public class EmitterStoredNotification : INotification
    {
        public EmitterStoredLog emitterStoredLog { get; set; }

        public EmitterStoredNotification(EmitterStoredLog emitterStoredLog)
        {
            this.emitterStoredLog = emitterStoredLog;
        }
    }
}
