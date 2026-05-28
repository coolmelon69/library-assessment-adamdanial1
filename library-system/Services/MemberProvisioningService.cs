using System.Security.Claims;
using library_system.Data;
using library_system.Dtos;
using library_system.Models;
using library_system.Security;
using Microsoft.EntityFrameworkCore;

namespace library_system.Services
{
    public class MemberProvisioningService : IMemberProvisioningService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<MemberProvisioningService> _logger;

        public MemberProvisioningService(
            ApplicationDbContext context,
            ILogger<MemberProvisioningService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<MemberProvisioningResult> GetOrProvisionMemberAsync(
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var ssoSubject = user.FindFirstValue("sub")?.Trim();
            var fullName = user.FindFirstValue("name")?.Trim();
            var email = user.FindFirstValue("email")?.Trim();

            if (string.IsNullOrWhiteSpace(ssoSubject)
                || string.IsNullOrWhiteSpace(fullName)
                || string.IsNullOrWhiteSpace(email))
            {
                _logger.LogWarning("Member provisioning failed because required Google claims were missing.");

                return MemberProvisioningResult.MissingClaims();
            }

            var member = await _context.Members
                .SingleOrDefaultAsync(existingMember => existingMember.SsoSubject == ssoSubject, cancellationToken);

            if (member is null)
            {
                member = new Member
                {
                    SsoSubject = ssoSubject,
                    FullName = fullName,
                    Email = email,
                    Role = AppRoles.User,
                    JoinedDate = DateTime.UtcNow
                };

                _context.Members.Add(member);
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Provisioned member {MemberId} from Google subject {SsoSubject}.", member.Id, ssoSubject);
            }
            else
            {
                _logger.LogDebug("Resolved existing member {MemberId} from Google subject {SsoSubject}.", member.Id, ssoSubject);
            }

            return MemberProvisioningResult.Success(new MemberProfileResponse(
                member.Id,
                member.FullName,
                member.Email,
                member.Role,
                member.JoinedDate));
        }
    }
}
