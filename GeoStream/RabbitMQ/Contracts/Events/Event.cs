namespace GeoStream.RabbitMQ.Contracts.Events
{
    public abstract class Event
    {
        public DateTime ReadTimestamp { get; set; }
    }
}
