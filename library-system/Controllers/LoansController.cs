using library_system.Dtos;
using library_system.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace library_system.Controllers
{
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("loans")]
    public class LoansController : ControllerBase
    {
        private readonly ILoanService _loanService;

        public LoansController(ILoanService loanService)
        {
            _loanService = loanService;
        }

        [HttpPost("{loanId:int}/return")]
        public async Task<ActionResult<LoanResponse>> ReturnLoan(
            int loanId,
            CancellationToken cancellationToken)
        {
            var result = await _loanService.ReturnLoanAsync(loanId, User, cancellationToken);

            return result.Status switch
            {
                ReturnLoanStatus.Succeeded => Ok(result.Loan),
                ReturnLoanStatus.LoanNotFound => NotFound(new { message = result.ErrorMessage }),
                ReturnLoanStatus.Forbidden => Forbid(JwtBearerDefaults.AuthenticationScheme),
                ReturnLoanStatus.MissingMemberClaims => BadRequest(new { message = result.ErrorMessage }),
                ReturnLoanStatus.AlreadyReturned => Conflict(new { message = result.ErrorMessage }),
                _ => BadRequest(new { message = "The return request could not be completed." })
            };
        }
    }
}
