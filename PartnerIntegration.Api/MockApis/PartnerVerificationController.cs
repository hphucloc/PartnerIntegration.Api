using Microsoft.AspNetCore.Mvc;

namespace PartnerIntegration.Api.MockApis
{
    [ApiController]
    [Route("api/[controller]")]
    public class PartnerVerificationController : ControllerBase
    {
        [HttpGet("verify/{partnerId}")]
        public ActionResult<bool> Verify(string partnerId)
        {
            if (string.IsNullOrWhiteSpace(partnerId))
            {
                return BadRequest(false);
            }

            if (Random.Shared.NextDouble() < 0.3)
            {
                throw new TimeoutException("Mock partner verification request timed out.");
            }

            return Ok(partnerId.StartsWith("P-"));
        }
    }
}
