using MediatR;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Text;
using TelemetryService.Core.Commands;
using TelemetryService.Core.Events;
using TelemetryService.Core.Interfaces;

namespace TelemetryService.Infrastructure.RabbitMQ
{
    public sealed class RabbitMQEventBus : IEventBus
    {
        private readonly ILogger<RabbitMQEventBus> _logger;
        private readonly IMediator _mediator;
        private readonly RabbitMQSettings _settings;

        public RabbitMQEventBus(IMediator mediator, IOptions<RabbitMQSettings> settings, ILogger<RabbitMQEventBus> logger)
        {
            _mediator = mediator;
            _settings = settings.Value;
            _logger = logger;
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
    }
}
