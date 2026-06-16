using Polly;
using Polly.Retry;

namespace PartnerIntegration.Api.Infrastructure.Resilience
{
    public static class PollyPolicies
    {
        public static AsyncRetryPolicy CreateRetryPolicy(int retryCount = 3)
        {
            return Policy.Handle<Exception>()
                .WaitAndRetryAsync(retryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
        }
    }
}
