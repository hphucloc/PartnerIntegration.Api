using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using System.Net.Http.Json;
using PartnerIntegration.Api.Services.Implementations;
using PartnerIntegration.Api.Services.Interfaces;
using Polly;

namespace PartnerIntegration.Tests.Services;

public class PartnerVerificationServiceTests
{
    [Fact]
    public async Task Should_Retry_When_Timeout_Occurs()
    {
        using var testContext = CreateService(
            [
                new TimeoutException("Request timed out."),
                CreateJsonResponse(HttpStatusCode.BadRequest, false)
            ],
            maxRetryAttempts: 3,
            out var handler);

        var result = await testContext.Service.VerifyPartnerAsync("P-1001");

        Assert.False(result);
        Assert.Equal(2, handler.AttemptCount);
    }

    [Fact]
    public async Task Should_Succeed_After_Retry()
    {
        using var testContext = CreateService(
            [
                new TimeoutException("Request timed out."),
                new TimeoutException("Request timed out again."),
                CreateJsonResponse(HttpStatusCode.OK, true)
            ],
            maxRetryAttempts: 3,
            out var handler);

        var result = await testContext.Service.VerifyPartnerAsync("P-1001");

        Assert.True(result);
        Assert.Equal(3, handler.AttemptCount);
    }

    [Fact]
    public async Task Should_Throw_After_Max_Retry()
    {
        using var testContext = CreateService(
            [
                new TimeoutException("Request timed out."),
                new TimeoutException("Request timed out again."),
                new TimeoutException("Request timed out for the last time.")
            ],
            maxRetryAttempts: 2,
            out var handler);

        await Assert.ThrowsAsync<TimeoutException>(() => testContext.Service.VerifyPartnerAsync("P-1001"));
        Assert.Equal(3, handler.AttemptCount);
    }

    [Fact]
    public async Task Should_ReturnFalse_When_ResponseBodyIsFalse()
    {
        using var testContext = CreateService(
            [
                CreateJsonResponse(HttpStatusCode.OK, false)
            ],
            maxRetryAttempts: 3,
            out _);

        var result = await testContext.Service.VerifyPartnerAsync("INVALID-123");

        Assert.False(result);
    }

    private static TestContext CreateService(
        IReadOnlyList<object> outcomes,
        int maxRetryAttempts,
        out SequencedHttpMessageHandler handler)
    {
        handler = new SequencedHttpMessageHandler(outcomes);

        var services = new ServiceCollection();
        services.AddSingleton(handler);
        services.AddHttpClient<IPartnerVerificationService, PartnerVerificationService>(
            client => client.BaseAddress = new Uri("https://example.test/"))
            .ConfigurePrimaryHttpMessageHandler(sp => sp.GetRequiredService<SequencedHttpMessageHandler>())
            .AddResilienceHandler(
                "partner-retry-test",
                builder =>
                {
                    builder.AddRetry(
                        new HttpRetryStrategyOptions
                        {
                            MaxRetryAttempts = maxRetryAttempts,
                            Delay = TimeSpan.Zero,
                            ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                                .Handle<TimeoutException>()
                        });
                });

        var provider = services.BuildServiceProvider();
        return new TestContext(provider, provider.GetRequiredService<IPartnerVerificationService>());
    }

    private sealed class TestContext : IDisposable
    {
        private readonly ServiceProvider _provider;

        public TestContext(ServiceProvider provider, IPartnerVerificationService service)
        {
            _provider = provider;
            Service = service;
        }

        public IPartnerVerificationService Service { get; }

        public void Dispose()
        {
            _provider.Dispose();
        }
    }

    private sealed class SequencedHttpMessageHandler : HttpMessageHandler
    {
        private readonly Queue<object> _outcomes;

        public SequencedHttpMessageHandler(IEnumerable<object> outcomes)
        {
            _outcomes = new Queue<object>(outcomes);
        }

        public int AttemptCount { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            AttemptCount++;

            if (_outcomes.Count == 0)
            {
                throw new InvalidOperationException("No outcome configured for this attempt.");
            }

            var outcome = _outcomes.Dequeue();

            if (outcome is Exception exception)
            {
                throw exception;
            }

            return Task.FromResult((HttpResponseMessage)outcome);
        }
    }

    private static HttpResponseMessage CreateJsonResponse(HttpStatusCode statusCode, bool isPartnerValid)
    {
        return new HttpResponseMessage(statusCode)
        {
            Content = JsonContent.Create(isPartnerValid)
        };
    }
}
