namespace TelemetryService.Core.Events
{
    public abstract class Event
    {
        public DateTime ReadTimestamp { get; protected set; }

        protected Event()
        {
            ReadTimestamp = DateTime.Now;
        }
    }
}
