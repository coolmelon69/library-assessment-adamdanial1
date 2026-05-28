namespace library_system.Dtos
{
    public record MemberProfileResponse(
        int Id,
        string FullName,
        string Email,
        string Role,
        DateTime JoinedDate);
}
