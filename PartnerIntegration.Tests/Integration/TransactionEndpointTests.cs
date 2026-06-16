using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PartnerIntegration.Api;
using PartnerIntegration.Api.Models.Requests;
using PartnerIntegration.Api.Models.Responses;
using PartnerIntegration.Api.Services.Interfaces;

namespace PartnerIntegration.Tests.Integration;

public class TransactionEndpointTests
{
    [Fact]
    public async Task Post_WithValidRequest_ReturnsOkResponse()
    {
        await using var factory = new TestApiFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/PartnerTransactions", new PartnerTransactionRequest
        {
            PartnerId = "PARTNER_001",
            TransactionId = "TX-1001",
            Amount = 250,
            Currency = "USD"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<string>>();
        Assert.NotNull(payload);
        Assert.True(payload!.Success);
        Assert.Equal("Transaction TX-1001 accepted.", payload.Data);
    }

    private sealed class TestApiFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<ITransactionService>();
                services.AddSingleton<ITransactionService, FakeTransactionService>();
            });
        }
    }

    private sealed class FakeTransactionService : ITransactionService
    {
        public Task<ApiResponse<string>> ProcessTransactionAsync(PartnerTransactionRequest request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(ApiResponse<string>.SuccessResponse($"Transaction {request.TransactionId} accepted."));
        }
    }
}
