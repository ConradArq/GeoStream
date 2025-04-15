using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Text;
using MessageProcessorService.Core.Commands;
using MessageProcessorService.Core.Events;
using MessageProcessorService.Core.Interfaces;
using RabbitMQ.Client.Events;

namespace MessageProcessorService.Infrastructure.RabbitMQ
{
    public sealed class RabbitMQEventBus : IEventBus
    {
        private readonly ILogger<RabbitMQEventBus> _logger;
        private readonly IMediator _mediator;
        private readonly RabbitMQSettings _settings;
        private readonly Dictionary<string, List<Type>> _handlers;
        private readonly List<Type> _eventTypes;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public RabbitMQEventBus(IMediator mediator, IOptions<RabbitMQSettings> settings, ILogger<RabbitMQEventBus> logger, IServiceScopeFactory serviceScopeFactory)
        {
            _mediator = mediator;
            _settings = settings.Value;
            _logger = logger;
            _handlers = new Dictionary<string, List<Type>>();
            _eventTypes = new List<Type>();
            _serviceScopeFactory = serviceScopeFactory;
        }

        public void Subscribe<T, TH>() where T : Event where TH : IEventHandler<T>
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
                throw new ArgumentException($"The handler exception {handlerType.Name} has already been registered previously by {eventName}");
            }

            _handlers[eventName].Add(handlerType);

            StartBasicConsume<T>();
        }

        public void Publish<T>(T @event) where T : Event
        {
            var factory = new ConnectionFactory()
            {
                HostName = _settings.Hostname,
                UserName = _settings.Username,
                Password = _settings.Password
            };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                var exchangeName = _settings.ExchangeName;

                channel.ExchangeDeclare(exchangeName, "direct", durable: false);

                var message = JsonConvert.SerializeObject(@event);
                var body = Encoding.UTF8.GetBytes(message);

                var routingKey = typeof(T).Name;

                _logger.LogInformation($"Publishing data to RabbitMQ Exchange ({exchangeName}) with RoutingKey ({routingKey})...");

                channel.BasicPublish(exchangeName, routingKey, basicProperties: null, body: body);

                _logger.LogInformation($"Scanner read data published to the RabbitMQ queue: {message}.");
            }
        }

        public Task SendCommand<T>(T command) where T : Command
        {
            return _mediator.Send(command);
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
                        if (eventType == null)
                        {
                            throw new ArgumentException($"Event type '{eventName}' not found.");
                        }

                        var concreteType = typeof(IEventHandler<>).MakeGenericType(eventType);

                        var @event = JsonConvert.DeserializeObject(message, eventType);
                        if (@event == null)
                        {
                            throw new ArgumentException($"Event of type '{eventName}' was not found or is null.");
                        }

                        var handleMethod = concreteType.GetMethod("Handle");
                        if (handleMethod == null)
                        {
                            throw new InvalidOperationException($"Handler does not contain a method 'Handle' for event type '{eventName}'.");
                        }

                        var res = (Task?)handleMethod.Invoke(handler, new object[] { @event });

                        if (res == null)
                        {
                            throw new InvalidOperationException($"The 'Handle' method for event type '{eventName}' did not return a valid Task.");
                        }
                    }
                }
            }
        }
    }
}
