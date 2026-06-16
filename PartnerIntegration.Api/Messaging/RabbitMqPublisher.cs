using PartnerIntegration.Api.Infrastructure.Configurations;

namespace PartnerIntegration.Api.Messaging
{
    public class RabbitMqPublisher : IMessagePublisher
    {
        private readonly RabbitMqOptions _options;

        public RabbitMqPublisher(RabbitMqOptions options)
        {
            _options = options;
        }

        public Task PublishAsync<T>(T message, CancellationToken cancellationToken = default)
        {
            // TODO: implement actual RabbitMQ publishing logic.
            return Task.CompletedTask;
        }
    }
}
