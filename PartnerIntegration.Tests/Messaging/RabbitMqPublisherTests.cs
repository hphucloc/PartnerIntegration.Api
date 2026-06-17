using System.Text.Json;
using PartnerIntegration.Api.Infrastructure.Configurations;
using PartnerIntegration.Api.Messaging;

namespace PartnerIntegration.Tests.Messaging;

public class RabbitMqPublisherTests
{
    [Fact]
    public async Task PublishAsync_SerializesMessageAndDelegatesToRabbitMqClient()
    {
        var client = new FakeRabbitMqClient();
        var publisher = new RabbitMqPublisher(
            new RabbitMqOptions
            {
                ExchangeName = "partner.exchange",
                ExchangeType = "direct",
                RoutingKey = "partner.transaction.accepted"
            },
            client);

        await publisher.PublishAsync(new TestMessage("TX-1", 125.5m));

        Assert.NotNull(client.PublishedMessage);
        Assert.Equal("partner.exchange", client.PublishedMessage!.ExchangeName);
        Assert.Equal("direct", client.PublishedMessage.ExchangeType);
        Assert.Equal("partner.transaction.accepted", client.PublishedMessage.RoutingKey);
        Assert.Equal("application/json", client.PublishedMessage.ContentType);

        using var payload = JsonDocument.Parse(client.PublishedMessage.Body);
        Assert.Equal("TX-1", payload.RootElement.GetProperty("transactionReference").GetString());
        Assert.Equal(125.5m, payload.RootElement.GetProperty("amount").GetDecimal());
    }

    private sealed record TestMessage(string TransactionReference, decimal Amount);

    private sealed class FakeRabbitMqClient : IRabbitMqClient
    {
        public RabbitMqOutboundMessage? PublishedMessage { get; private set; }

        public Task PublishAsync(RabbitMqOutboundMessage message, CancellationToken cancellationToken = default)
        {
            PublishedMessage = message;
            return Task.CompletedTask;
        }
    }
}
