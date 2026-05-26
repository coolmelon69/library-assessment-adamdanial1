using library_system.Dtos;
using library_system.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace library_system.Controllers
{
    [ApiController]
    [Authorize]
    [Route("me")]
    public class MeController : ControllerBase
    {
        private readonly IMemberProvisioningService _memberProvisioningService;

        public MeController(IMemberProvisioningService memberProvisioningService)
        {
            _memberProvisioningService = memberProvisioningService;
        }

        [HttpGet]
        public async Task<ActionResult<MemberProfileResponse>> GetMe(CancellationToken cancellationToken)
        {
            var result = await _memberProvisioningService.GetOrProvisionMemberAsync(User, cancellationToken);

            if (!result.Succeeded)
            {
                return BadRequest(new { message = result.ErrorMessage });
            }

            return Ok(result.Member);
        }
    }
}
