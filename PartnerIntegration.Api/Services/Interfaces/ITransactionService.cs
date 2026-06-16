using PartnerIntegration.Api.Models.Requests;
using PartnerIntegration.Api.Models.Responses;

namespace PartnerIntegration.Api.Services.Interfaces
{
    public interface ITransactionService
    {
        Task<ApiResponse<string>> ProcessTransactionAsync(PartnerTransactionRequest request, CancellationToken cancellationToken = default);
    }
}
