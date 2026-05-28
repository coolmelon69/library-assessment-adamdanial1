using System.Security.Claims;
using library_system.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;

namespace library_system.Security
{
    public class MemberRoleClaimsTransformation : IClaimsTransformation
    {
        private readonly ApplicationDbContext _context;

        public MemberRoleClaimsTransformation(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            if (principal.HasClaim(claim => claim.Type == ClaimTypes.Role))
            {
                return principal;
            }

            var ssoSubject = principal.FindFirstValue("sub")?.Trim();
            if (string.IsNullOrWhiteSpace(ssoSubject))
            {
                return principal;
            }

            var role = await _context.Members
                .AsNoTracking()
                .Where(member => member.SsoSubject == ssoSubject)
                .Select(member => member.Role)
                .SingleOrDefaultAsync();

            if (string.IsNullOrWhiteSpace(role))
            {
                return principal;
            }

            var currentIdentity = principal.Identity as ClaimsIdentity;
            var identity = new ClaimsIdentity(
                authenticationType: nameof(MemberRoleClaimsTransformation),
                nameType: currentIdentity?.NameClaimType ?? ClaimTypes.Name,
                roleType: ClaimTypes.Role);

            identity.AddClaim(new Claim(ClaimTypes.Role, role));
            principal.AddIdentity(identity);

            return principal;
        }
    }
}
