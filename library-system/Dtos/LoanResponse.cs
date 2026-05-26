namespace library_system.Dtos
{
    public record LoanResponse(
        int Id,
        int BookId,
        string BookTitle,
        string BookAuthor,
        int MemberId,
        DateTime BorrowedDate,
        DateTime? ReturnedDate);
}
