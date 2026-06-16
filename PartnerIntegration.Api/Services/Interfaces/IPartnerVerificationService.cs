namespace PartnerIntegration.Api.Services.Interfaces
{
    public interface IPartnerVerificationService
    {
        Task<bool> VerifyPartnerAsync(string partnerId, CancellationToken cancellationToken = default);
    }
}
