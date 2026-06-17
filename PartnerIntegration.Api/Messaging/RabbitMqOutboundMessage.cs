namespace PartnerIntegration.Api.Messaging
{
    public sealed record RabbitMqOutboundMessage(
        string ExchangeName,
        string ExchangeType,
        string RoutingKey,
        byte[] Body,
        string ContentType);
}
