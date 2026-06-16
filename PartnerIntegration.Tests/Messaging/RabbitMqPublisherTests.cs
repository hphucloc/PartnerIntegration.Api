using PartnerIntegration.Api.Infrastructure.Configurations;
using PartnerIntegration.Api.Messaging;

namespace PartnerIntegration.Tests.Messaging;

public class RabbitMqPublisherTests
{
    [Fact]
    public async Task PublishAsync_CompletesSuccessfully()
    {
        var publisher = new RabbitMqPublisher(new RabbitMqOptions
        {
            HostName = "localhost",
            Port = 5672,
            UserName = "guest",
            Password = "guest",
            ExchangeName = "partner.exchange"
        });

        await publisher.PublishAsync(new { Id = "TX-1" });

        Assert.True(true);
    }
}
