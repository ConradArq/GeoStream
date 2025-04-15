namespace GeoStream.UIEventsMediator
{
    public interface IEventAggregator
    {
        void Publish<TEvent>(TEvent eventToPublish);
        void Subscribe<TEvent>(Action<TEvent> action);
        void Unsubscribe<TEvent>(Action<TEvent> action);
    }
}
