namespace GeoStream.RabbitMQ.Bus
{
    public class RabbitMQSettings
    {
        public string Hostname { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ExchangeName { get; set; } = string.Empty;
    }
}
