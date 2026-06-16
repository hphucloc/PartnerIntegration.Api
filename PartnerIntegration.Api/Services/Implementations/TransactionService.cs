using PartnerIntegration.Api.Models.Requests;
using PartnerIntegration.Api.Models.Responses;
using PartnerIntegration.Api.Services.Interfaces;
using PartnerIntegration.Api.Validators;

namespace PartnerIntegration.Api.Services.Implementations
{
    public class TransactionService : ITransactionService
    {
        private readonly IPartnerVerificationService _partnerVerificationService;
        private readonly PartnerTransactionValidator _validator;

        public TransactionService(IPartnerVerificationService partnerVerificationService)
        {
            _partnerVerificationService = partnerVerificationService;
            _validator = new PartnerTransactionValidator();
        }

        public async Task<ApiResponse<string>> ProcessTransactionAsync(PartnerTransactionRequest request, CancellationToken cancellationToken = default)
        {
            if (!_validator.Validate(request, out var errors))
            {
                return ApiResponse<string>.ErrorResponse(errors);
            }

            var isPartnerValid = await _partnerVerificationService.VerifyPartnerAsync(request.PartnerId, cancellationToken);
            if (!isPartnerValid)
            {
                return ApiResponse<string>.ErrorResponse(new[] { "Partner verification failed." });
            }

            return ApiResponse<string>.SuccessResponse($"Transaction {request.TransactionId} accepted.");
        }
    }
}
