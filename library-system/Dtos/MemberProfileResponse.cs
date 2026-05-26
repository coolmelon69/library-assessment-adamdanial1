namespace library_system.Dtos
{
    public record MemberProfileResponse(
        int Id,
        string FullName,
        string Email,
        DateTime JoinedDate);
}
