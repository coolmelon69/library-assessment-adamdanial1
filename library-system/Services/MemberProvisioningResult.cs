using library_system.Dtos;

namespace library_system.Services
{
    public record MemberProvisioningResult(
        bool Succeeded,
        MemberProfileResponse? Member,
        string? ErrorMessage)
    {
        public static MemberProvisioningResult Success(MemberProfileResponse member)
        {
            return new MemberProvisioningResult(true, member, null);
        }

        public static MemberProvisioningResult MissingClaims()
        {
            return new MemberProvisioningResult(
                false,
                null,
                "Authenticated token must include sub, name, and email claims.");
        }
    }
}
