
namespace GeoStream.RabbitMQ.Models
{
    public class ScannerConnectionStateLog
    {
        public DateTime ReadTimestamp { get; set; }
        public string ScannerCode { get; set; } = string.Empty;
        public bool IsConnected { get; set; }
    }
}
