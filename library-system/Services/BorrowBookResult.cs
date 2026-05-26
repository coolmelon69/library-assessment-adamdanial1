using library_system.Dtos;

namespace library_system.Services
{
    public enum BorrowBookStatus
    {
        Succeeded,
        MissingMemberClaims,
        BookNotFound,
        NoCopiesAvailable,
        ActiveLoanLimitReached
    }

    public record BorrowBookResult(
        BorrowBookStatus Status,
        LoanResponse? Loan,
        string? ErrorMessage)
    {
        public static BorrowBookResult Success(LoanResponse loan)
        {
            return new BorrowBookResult(BorrowBookStatus.Succeeded, loan, null);
        }

        public static BorrowBookResult Failure(BorrowBookStatus status, string message)
        {
            return new BorrowBookResult(status, null, message);
        }
    }
}
