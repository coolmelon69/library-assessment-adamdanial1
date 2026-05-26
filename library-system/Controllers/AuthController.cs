using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;

namespace library_system.Controllers
{
    [Route("auth")]
    public class AuthController : Controller
    {
        [HttpGet("sign-in")]
        public IActionResult SignIn(string? returnUrl = null)
        {
            var redirectUri = Url.IsLocalUrl(returnUrl)
                ? returnUrl
                : Url.Action("Index", "Home");

            return Challenge(
                new AuthenticationProperties { RedirectUri = redirectUri },
                OpenIdConnectDefaults.AuthenticationScheme);
        }

        [HttpPost("sign-out")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignOutUser()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Index", "Home");
        }

        [HttpGet("access-denied")]
        public IActionResult AccessDenied()
        {
            return Forbid();
        }
    }
}
