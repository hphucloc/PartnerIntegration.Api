namespace PartnerIntegration.Api.Messaging
{
    public interface IRabbitMqClient
    {
        Task PublishAsync(RabbitMqOutboundMessage message, CancellationToken cancellationToken = default);
    }
}
