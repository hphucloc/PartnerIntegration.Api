using PartnerIntegration.Api.Infrastructure.Configurations;
using RabbitMQ.Client;

namespace PartnerIntegration.Api.Messaging
{
    public class RabbitMqClient : IRabbitMqClient
    {
        private readonly RabbitMqOptions _options;

        public RabbitMqClient(RabbitMqOptions options)
        {
            _options = options;
        }

        public Task PublishAsync(RabbitMqOutboundMessage message, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var factory = new ConnectionFactory
            {
                HostName = _options.HostName,
                Port = _options.Port,
                UserName = _options.UserName,
                Password = _options.Password
            };

            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.ExchangeDeclare(
                exchange: message.ExchangeName,
                type: message.ExchangeType,
                durable: true,
                autoDelete: false);

            var properties = channel.CreateBasicProperties();
            properties.ContentType = message.ContentType;
            properties.DeliveryMode = 2;

            channel.BasicPublish(
                exchange: message.ExchangeName,
                routingKey: message.RoutingKey,
                basicProperties: properties,
                body: message.Body);

            return Task.CompletedTask;
        }
    }
}
