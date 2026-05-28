using System.Security.Claims;
using library_system.Services;
using library_system.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace library_system.Controllers
{
    [Route("me")]
    public class MeController : Controller
    {
        private const int ActiveLoanLimit = 3;

        private readonly IMemberProvisioningService _memberProvisioningService;
        private readonly ILoanService _loanService;

        public MeController(
            IMemberProvisioningService memberProvisioningService,
            ILoanService loanService)
        {
            _memberProvisioningService = memberProvisioningService;
            _loanService = loanService;
        }

        [HttpGet("")]
        public async Task<IActionResult> GetMe(CancellationToken cancellationToken)
        {
            if (!IsBrowserHtmlRequest())
            {
                var bearerPrincipal = await AuthenticateBearerAsync();
                if (bearerPrincipal is null)
                {
                    return Challenge(JwtBearerDefaults.AuthenticationScheme);
                }

                var apiResult = await _memberProvisioningService.GetOrProvisionMemberAsync(bearerPrincipal, cancellationToken);

                if (!apiResult.Succeeded)
                {
                    return BadRequest(new { message = apiResult.ErrorMessage });
                }

                return Ok(apiResult.Member);
            }

            var browserPrincipal = await AuthenticateBrowserAsync();
            if (browserPrincipal is null)
            {
                return ChallengeBrowser();
            }

            var profileResult = await _memberProvisioningService.GetOrProvisionMemberAsync(browserPrincipal, cancellationToken);
            var loans = await _loanService.GetActiveLoansForCurrentMemberAsync(browserPrincipal, cancellationToken);

            return View("Profile", new MeProfileViewModel
            {
                Member = profileResult.Member,
                ActiveLoans = loans,
                ActiveLoanLimit = ActiveLoanLimit,
                ErrorMessage = profileResult.Succeeded
                    ? null
                    : profileResult.ErrorMessage ?? "Your Google sign-in could not be resolved."
            });
        }

        [HttpGet("loans")]
        public async Task<IActionResult> GetMyActiveLoans(CancellationToken cancellationToken)
        {
            if (!IsBrowserHtmlRequest())
            {
                var bearerPrincipal = await AuthenticateBearerAsync();
                if (bearerPrincipal is null)
                {
                    return Challenge(JwtBearerDefaults.AuthenticationScheme);
                }

                var apiLoans = await _loanService.GetActiveLoansForCurrentMemberAsync(bearerPrincipal, cancellationToken);

                return Ok(apiLoans);
            }

            var browserPrincipal = await AuthenticateBrowserAsync();
            if (browserPrincipal is null)
            {
                return ChallengeBrowser();
            }

            var profileResult = await _memberProvisioningService.GetOrProvisionMemberAsync(browserPrincipal, cancellationToken);
            var loans = await _loanService.GetActiveLoansForCurrentMemberAsync(browserPrincipal, cancellationToken);

            return View("Loans", new MeLoansViewModel
            {
                Member = profileResult.Member,
                Loans = loans,
                ErrorMessage = profileResult.Succeeded
                    ? null
                    : profileResult.ErrorMessage ?? "Your Google sign-in could not be resolved."
            });
        }

        [HttpPost("loans/{loanId:int}/return")]
        [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReturnLoan(int loanId, CancellationToken cancellationToken)
        {
            var result = await _loanService.ReturnLoanAsync(loanId, User, cancellationToken);

            switch (result.Status)
            {
                case ReturnLoanStatus.Succeeded:
                    TempData["MeSuccessMessage"] = "Loan returned successfully.";
                    break;

                case ReturnLoanStatus.LoanNotFound:
                    TempData["MeErrorMessage"] = "That loan could not be found.";
                    break;

                case ReturnLoanStatus.Forbidden:
                    TempData["MeErrorMessage"] = "You can only return your own loans.";
                    break;

                case ReturnLoanStatus.MissingMemberClaims:
                    TempData["MeErrorMessage"] = "Your Google sign-in could not be resolved. Please sign out and sign in again.";
                    break;

                case ReturnLoanStatus.AlreadyReturned:
                    TempData["MeErrorMessage"] = "This loan has already been returned.";
                    break;

                default:
                    TempData["MeErrorMessage"] = "The return request could not be completed.";
                    break;
            }

            return RedirectToAction(nameof(GetMyActiveLoans));
        }

        private bool IsBrowserHtmlRequest()
        {
            var authorization = Request.Headers.Authorization.ToString();
            if (authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var accept = Request.Headers.Accept.ToString();
            return accept.Contains("text/html", StringComparison.OrdinalIgnoreCase);
        }

        private async Task<ClaimsPrincipal?> AuthenticateBearerAsync()
        {
            var result = await HttpContext.AuthenticateAsync(JwtBearerDefaults.AuthenticationScheme);
            return result.Succeeded
                ? result.Principal
                : null;
        }

        private async Task<ClaimsPrincipal?> AuthenticateBrowserAsync()
        {
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return result.Succeeded
                ? result.Principal
                : null;
        }

        private ChallengeResult ChallengeBrowser()
        {
            return Challenge(
                new AuthenticationProperties { RedirectUri = Request.Path + Request.QueryString },
                OpenIdConnectDefaults.AuthenticationScheme);
        }
    }
}
