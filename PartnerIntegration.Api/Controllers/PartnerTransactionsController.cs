using Microsoft.AspNetCore.Mvc;
using PartnerIntegration.Api.Models.Requests;
using PartnerIntegration.Api.Models.Responses;
using PartnerIntegration.Api.Services.Interfaces;

namespace PartnerIntegration.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PartnerTransactionsController : ControllerBase
    {
        private readonly ITransactionService _transactionService;

        public PartnerTransactionsController(ITransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<string>>> Post([FromBody] PartnerTransactionRequest request, CancellationToken cancellationToken)
        {
            var response = await _transactionService.ProcessTransactionAsync(request, cancellationToken);
            return response.Success ? Ok(response) : BadRequest(response);
        }
    }
}
