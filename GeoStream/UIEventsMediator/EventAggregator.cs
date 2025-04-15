namespace GeoStream.UIEventsMediator
{
    public class EventAggregator : IEventAggregator
    {
        private readonly Dictionary<Type, List<Delegate>> listeners = new();

        public void Publish<TEvent>(TEvent eventToPublish)
        {
            if (listeners.TryGetValue(typeof(TEvent), out var subscribers))
            {
                foreach (var subscriber in subscribers.OfType<Action<TEvent>>())
                {
                    subscriber(eventToPublish);
                }
            }
        }

        public void Subscribe<TEvent>(Action<TEvent> action)
        {
            if (!listeners.ContainsKey(typeof(TEvent)))
            {
                listeners[typeof(TEvent)] = new List<Delegate>();
            }
            listeners[typeof(TEvent)].Add(action);
        }

        public void Unsubscribe<TEvent>(Action<TEvent> action)
        {
            if (listeners.ContainsKey(typeof(TEvent)))
            {
                var subscribers = listeners[typeof(TEvent)];
                subscribers.Remove(action);

                if (!subscribers.Any())
                {
                    listeners.Remove(typeof(TEvent));
                }
            }
        }
    }
}
