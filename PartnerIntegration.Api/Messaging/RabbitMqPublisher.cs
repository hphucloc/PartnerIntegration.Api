using PartnerIntegration.Api.Infrastructure.Configurations;
using System.Text.Json;

namespace PartnerIntegration.Api.Messaging
{
    public class RabbitMqPublisher : IMessagePublisher
    {
        private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

        private readonly IRabbitMqClient _client;
        private readonly RabbitMqOptions _options;

        public RabbitMqPublisher(RabbitMqOptions options, IRabbitMqClient client)
        {
            _options = options;
            _client = client;
        }

        public Task PublishAsync<T>(T message, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(message);

            var body = JsonSerializer.SerializeToUtf8Bytes(message, SerializerOptions);

            return _client.PublishAsync(
                new RabbitMqOutboundMessage(
                    _options.ExchangeName,
                    _options.ExchangeType,
                    _options.RoutingKey,
                    body,
                    "application/json"),
                cancellationToken);
        }
    }
}
