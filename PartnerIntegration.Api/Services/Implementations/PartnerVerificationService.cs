using PartnerIntegration.Api.Services.Interfaces;

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
            var response =
                await _httpClient.GetAsync(
                    $"api/PartnerVerification/verify/{partnerId}",
                    cancellationToken);

            return response.IsSuccessStatusCode;       
        }
    }
}
