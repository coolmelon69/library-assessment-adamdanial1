using System.Security.Claims;

namespace library_system.Services.Members
{
    public interface IMemberProvisioningService
    {
        Task<MemberProvisioningResult> GetOrProvisionMemberAsync(
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default);
    }
}
