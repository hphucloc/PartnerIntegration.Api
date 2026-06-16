using PartnerIntegration.Api.Services.Interfaces;

namespace PartnerIntegration.Api.Services.Implementations
{
    public class PartnerVerificationService : IPartnerVerificationService
    {
        public Task<bool> VerifyPartnerAsync(string partnerId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(!string.IsNullOrWhiteSpace(partnerId) && partnerId.StartsWith("PARTNER_"));
        }
    }
}
