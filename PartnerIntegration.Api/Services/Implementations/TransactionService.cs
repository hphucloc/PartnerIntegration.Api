using PartnerIntegration.Api.Messaging;
using PartnerIntegration.Api.Models.Events;
using PartnerIntegration.Api.Models.Requests;
using PartnerIntegration.Api.Models.Responses;
using PartnerIntegration.Api.Services.Interfaces;
using PartnerIntegration.Api.Validators;

namespace PartnerIntegration.Api.Services.Implementations
{
    public class TransactionService : ITransactionService
    {
        private readonly IMessagePublisher _messagePublisher;
        private readonly IPartnerVerificationService _partnerVerificationService;
        private readonly PartnerTransactionValidator _validator;

        public TransactionService(
            IPartnerVerificationService partnerVerificationService,
            IMessagePublisher messagePublisher)
        {
            _messagePublisher = messagePublisher;
            _partnerVerificationService = partnerVerificationService;
            _validator = new PartnerTransactionValidator();
        }

        public async Task<ApiResponse<string>> ProcessTransactionAsync(PartnerTransactionRequest request, CancellationToken cancellationToken = default)
        {
            var validationResult = _validator.Validate(request);
            if (!validationResult.IsValid)
            {
                return ApiResponse<string>.ErrorResponse(validationResult.Errors.Select(error => error.ErrorMessage));
            }

            var isPartnerValid = await _partnerVerificationService.VerifyPartnerAsync(request.PartnerId, cancellationToken);
            if (!isPartnerValid)
            {
                return ApiResponse<string>.ErrorResponse(new[] { "Partner verification failed." });
            }

            await _messagePublisher.PublishAsync(
                new PartnerTransactionAcceptedEvent(
                    request.PartnerId,
                    request.TransactionReference,
                    request.Amount,
                    request.Currency,
                    request.Timestamp,
                    DateTime.UtcNow),
                cancellationToken);

            return ApiResponse<string>.SuccessResponse($"Transaction {request.TransactionReference} accepted.");
        }
    }
}
