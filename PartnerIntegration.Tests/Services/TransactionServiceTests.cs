using PartnerIntegration.Api.Messaging;
using PartnerIntegration.Api.Models.Events;
using PartnerIntegration.Api.Models.Requests;
using PartnerIntegration.Api.Services.Implementations;
using PartnerIntegration.Api.Services.Interfaces;

namespace PartnerIntegration.Tests.Services;

public class TransactionServiceTests
{
    [Fact]
    public async Task ProcessTransactionAsync_WhenRequestIsValid_PublishesAcceptedEvent()
    {
        var publisher = new FakeMessagePublisher();
        var service = new TransactionService(new FakePartnerVerificationService(true), publisher);
        var request = new PartnerTransactionRequest
        {
            PartnerId = "P-1001",
            TransactionReference = "TX-1001",
            Amount = 250m,
            Currency = "USD",
            Timestamp = new DateTime(2024, 5, 10, 14, 30, 0, DateTimeKind.Utc)
        };

        var response = await service.ProcessTransactionAsync(request);

        Assert.True(response.Success);
        var publishedEvent = Assert.IsType<PartnerTransactionAcceptedEvent>(publisher.PublishedMessage);
        Assert.Equal(request.PartnerId, publishedEvent.PartnerId);
        Assert.Equal(request.TransactionReference, publishedEvent.TransactionReference);
        Assert.Equal(request.Amount, publishedEvent.Amount);
        Assert.Equal(request.Currency, publishedEvent.Currency);
        Assert.Equal(request.Timestamp, publishedEvent.Timestamp);
    }

    [Fact]
    public async Task ProcessTransactionAsync_WhenPartnerVerificationFails_DoesNotPublishMessage()
    {
        var publisher = new FakeMessagePublisher();
        var service = new TransactionService(new FakePartnerVerificationService(false), publisher);
        var request = new PartnerTransactionRequest
        {
            PartnerId = "P-1001",
            TransactionReference = "TX-1001",
            Amount = 250m,
            Currency = "USD"
        };

        var response = await service.ProcessTransactionAsync(request);

        Assert.False(response.Success);
        Assert.Null(publisher.PublishedMessage);
    }

    [Fact]
    public async Task ProcessTransactionAsync_WhenValidationFails_DoesNotPublishMessage()
    {
        var publisher = new FakeMessagePublisher();
        var service = new TransactionService(new FakePartnerVerificationService(true), publisher);
        var request = new PartnerTransactionRequest();

        var response = await service.ProcessTransactionAsync(request);

        Assert.False(response.Success);
        Assert.Null(publisher.PublishedMessage);
    }

    private sealed class FakePartnerVerificationService(bool isValid) : IPartnerVerificationService
    {
        public Task<bool> VerifyPartnerAsync(string partnerId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(isValid);
        }
    }

    private sealed class FakeMessagePublisher : IMessagePublisher
    {
        public object? PublishedMessage { get; private set; }

        public Task PublishAsync<T>(T message, CancellationToken cancellationToken = default)
        {
            PublishedMessage = message;
            return Task.CompletedTask;
        }
    }
}
