using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using PartnerIntegration.Api.Services.Implementations;

namespace PartnerIntegration.Tests.Services;

public class PartnerVerificationServiceTests
{
    [Fact]
    public async Task VerifyPartnerAsync_WhenApiReturnsSuccess_ReturnsTrue()
    {
        var service = CreateService(HttpStatusCode.OK);

        var result = await service.VerifyPartnerAsync("PARTNER_123");

        Assert.True(result);
    }

    [Fact]
    public async Task VerifyPartnerAsync_WhenApiReturnsFailure_ReturnsFalse()
    {
        var service = CreateService(HttpStatusCode.BadRequest);

        var result = await service.VerifyPartnerAsync("INVALID_123");

        Assert.False(result);
    }

    private static PartnerVerificationService CreateService(HttpStatusCode statusCode)
    {
        var httpClient = new HttpClient(new StubHttpMessageHandler(statusCode))
        {
            BaseAddress = new Uri("https://localhost:7001/")
        };

        return new PartnerVerificationService(httpClient);
    }

    private sealed class StubHttpMessageHandler(HttpStatusCode statusCode) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage(statusCode));
        }
    }
}
