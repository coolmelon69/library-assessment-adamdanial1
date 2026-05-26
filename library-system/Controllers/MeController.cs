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
        private readonly ILoanService _loanService;

        public MeController(
            IMemberProvisioningService memberProvisioningService,
            ILoanService loanService)
        {
            _memberProvisioningService = memberProvisioningService;
            _loanService = loanService;
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

        [HttpGet("loans")]
        public async Task<ActionResult<IReadOnlyList<LoanResponse>>> GetMyActiveLoans(CancellationToken cancellationToken)
        {
            var loans = await _loanService.GetActiveLoansForCurrentMemberAsync(User, cancellationToken);

            return Ok(loans);
        }
    }
}
