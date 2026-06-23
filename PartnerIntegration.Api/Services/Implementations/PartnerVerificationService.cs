using PartnerIntegration.Api.Services.Interfaces;
using System.Net.Http.Json;

namespace PartnerIntegration.Api.Services.Implementations
{
    public class PartnerVerificationService : IPartnerVerificationService
    {
        private readonly HttpClient _httpClient;

        public PartnerVerificationService(
            HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<bool> VerifyPartnerAsync(string partnerId, CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.GetAsync(
                $"api/PartnerVerification/verify/{partnerId}",
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return false;
            }

            var isValid = await response.Content.ReadFromJsonAsync<bool?>(cancellationToken: cancellationToken);
            return isValid ?? false;
        }
    }
}
