using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
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
        await using var factory = new TestApiFactory<FakeTransactionService>();
        using var client = factory.CreateAuthorizedClient();

        var response = await PostAuthorizedAsync(client, new PartnerTransactionRequest
        {
            PartnerId = "P-1001",
            TransactionReference = "TX-1001",
            Amount = 250,
            Currency = "USD"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<string>>();
        Assert.NotNull(payload);
        Assert.True(payload!.Success);
        Assert.Equal("Transaction TX-1001 accepted.", payload.Data);
    }

    [Fact]
    public async Task Post_WhenTimeoutExceptionOccurs_ReturnsGatewayTimeoutProblemDetails()
    {
        await using var factory = new TestApiFactory<TimeoutTransactionService>();
        using var client = factory.CreateAuthorizedClient();

        var response = await PostAuthorizedAsync(client, new PartnerTransactionRequest
        {
            PartnerId = "P-1001",
            TransactionReference = "TX-1001",
            Amount = 250,
            Currency = "USD"
        });

        Assert.Equal(HttpStatusCode.GatewayTimeout, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(payload);
        Assert.Equal("The request timed out.", payload!.Title);
        Assert.Equal(StatusCodes.Status504GatewayTimeout, payload.Status);
        Assert.Equal("/api/PartnerTransactions", payload.Instance);
    }

    [Fact]
    public async Task Post_WhenUnhandledExceptionOccurs_ReturnsInternalServerErrorProblemDetails()
    {
        await using var factory = new TestApiFactory<ThrowingTransactionService>();
        using var client = factory.CreateAuthorizedClient();

        var response = await PostAuthorizedAsync(client, new PartnerTransactionRequest
        {
            PartnerId = "P-1001",
            TransactionReference = "TX-1001",
            Amount = 250,
            Currency = "USD"
        });

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(payload);
        Assert.Equal("An unexpected error occurred.", payload!.Title);
        Assert.Equal(StatusCodes.Status500InternalServerError, payload.Status);
        Assert.Equal("/api/PartnerTransactions", payload.Instance);
    }

    [Fact]
    public async Task Post_WhenApiKeyIsMissing_ReturnsUnauthorizedProblemDetails()
    {
        await using var factory = new TestApiFactory<FakeTransactionService>();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/PartnerTransactions", new PartnerTransactionRequest
        {
            PartnerId = "P-1001",
            TransactionReference = "TX-1001",
            Amount = 250,
            Currency = "USD"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(payload);
        Assert.Equal("Unauthorized request.", payload!.Title);
        Assert.Equal(StatusCodes.Status401Unauthorized, payload.Status);
    }

    [Fact]
    public async Task Post_WhenRateLimitExceeded_ReturnsTooManyRequestsProblemDetails()
    {
        await using var factory = new TestApiFactory<FakeTransactionService>();
        using var client = factory.CreateAuthorizedClient();

        HttpResponseMessage? lastResponse = null;

        for (var attempt = 0; attempt < 11; attempt++)
        {
            lastResponse = await PostAuthorizedAsync(client, CreateValidRequest());
        }

        Assert.NotNull(lastResponse);
        Assert.Equal((HttpStatusCode)StatusCodes.Status429TooManyRequests, lastResponse!.StatusCode);

        var payload = await lastResponse.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(payload);
        Assert.Equal("Too many requests.", payload!.Title);
        Assert.Equal(StatusCodes.Status429TooManyRequests, payload.Status);
    }

    private sealed class TestApiFactory<TTransactionService> : WebApplicationFactory<Program>
        where TTransactionService : class, ITransactionService
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureLogging(logging => logging.ClearProviders());
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<ITransactionService>();
                services.AddSingleton<ITransactionService, TTransactionService>();
            });
        }

        public HttpClient CreateAuthorizedClient()
        {
            var client = CreateClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return client;
        }
    }

    private sealed class FakeTransactionService : ITransactionService
    {
        public Task<ApiResponse<string>> ProcessTransactionAsync(PartnerTransactionRequest request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(ApiResponse<string>.SuccessResponse($"Transaction {request.TransactionReference} accepted."));
        }
    }

    private sealed class TimeoutTransactionService : ITransactionService
    {
        public Task<ApiResponse<string>> ProcessTransactionAsync(PartnerTransactionRequest request, CancellationToken cancellationToken = default)
        {
            throw new TimeoutException("Mock timeout from transaction service.");
        }
    }

    private sealed class ThrowingTransactionService : ITransactionService
    {
        public Task<ApiResponse<string>> ProcessTransactionAsync(PartnerTransactionRequest request, CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException("Mock unhandled exception.");
        }
    }

    private static PartnerTransactionRequest CreateValidRequest()
    {
        return new PartnerTransactionRequest
        {
            PartnerId = "P-1001",
            TransactionReference = $"TX-{Guid.NewGuid():N}",
            Amount = 250,
            Currency = "USD"
        };
    }

    private static Task<HttpResponseMessage> PostAuthorizedAsync(HttpClient client, PartnerTransactionRequest request)
    {
        var message = new HttpRequestMessage(HttpMethod.Post, "/api/PartnerTransactions")
        {
            Content = JsonContent.Create(request)
        };
        message.Headers.Add("X-Api-Key", "x-api-key");

        return client.SendAsync(message);
    }
}
