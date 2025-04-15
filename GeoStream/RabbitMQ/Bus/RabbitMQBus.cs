using Microsoft.Extensions.Options;
using GeoStream.RabbitMQ.Contracts.Bus;
using GeoStream.RabbitMQ.Contracts.Events;
using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using System.Text;
using Newtonsoft.Json;

namespace GeoStream.RabbitMQ.Bus
{
    public sealed class RabbitMQBus : IEventBus
    {
        private readonly RabbitMQSettings _settings;
        private readonly Dictionary<string, List<Type>> _handlers;
        private readonly List<Type> _eventTypes;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public RabbitMQBus(IServiceScopeFactory serviceScopeFactory, IOptions<RabbitMQSettings> settings)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _handlers = new Dictionary<string, List<Type>>();
            _eventTypes = new List<Type>();
            _settings = settings.Value;
        }    

        public void Suscribe<T, TH>()
            where T : Event
            where TH : IEventHandler<T>
        {

            var eventName = typeof(T).Name;
            var handlerType = typeof(TH);

            if (!_eventTypes.Contains(typeof(T)))
            {
                _eventTypes.Add(typeof(T));
            }

            if (!_handlers.ContainsKey(eventName))
            {
                _handlers.Add(eventName, new List<Type>());
            }

            if (_handlers[eventName].Any(s => s.GetType() == handlerType))
            {
                throw new ArgumentException($"El handler exception {handlerType.Name} ya fue registrado anteriormente por {eventName}");
            }

            _handlers[eventName].Add(handlerType);

            StartBasicConsume<T>();

        }

        private void StartBasicConsume<T>() where T : Event
        {
            var factory = new ConnectionFactory()
            {
                HostName = _settings.Hostname,
                UserName = _settings.Username,
                Password = _settings.Password
            };
            var connection = factory.CreateConnection();
            var channel = connection.CreateModel();


            var exchangeName = _settings.ExchangeName;
            channel.ExchangeDeclare(exchangeName, "direct", durable: false);

            var queueName = channel.QueueDeclare().QueueName;
            channel.QueueBind(queueName, exchangeName, typeof(T).Name);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                var message = Encoding.UTF8.GetString(ea.Body.ToArray());
                if (ea.RoutingKey == typeof(T).Name)
                {
                    ProcessEvent(typeof(T).Name, message);
                }
            };

            channel.BasicConsume(queueName, autoAck: true, consumer);
        }      

        private void ProcessEvent(string eventName, string message)
        {
            if (_handlers.ContainsKey(eventName))
            {

                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var suscriptions = _handlers[eventName];
                    foreach (var subscription in suscriptions)
                    {
                        var handler = scope.ServiceProvider.GetService(subscription);
                        if (handler == null) continue;
                        var eventType = _eventTypes.SingleOrDefault(t => t.Name == eventName);
                        var @event = JsonConvert.DeserializeObject(message, eventType);
                        var concreteType = typeof(IEventHandler<>).MakeGenericType(eventType);

                        var res = (Task)concreteType.GetMethod("Handle").Invoke(handler, new object[] { @event });

                    }
                }
            }
        }
    }
}
