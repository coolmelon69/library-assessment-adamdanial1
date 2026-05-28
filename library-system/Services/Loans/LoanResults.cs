using library_system.Dtos;

namespace library_system.Services.Loans
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

    public enum ReturnLoanStatus
    {
        Succeeded,
        MissingMemberClaims,
        LoanNotFound,
        Forbidden,
        AlreadyReturned
    }

    public record ReturnLoanResult(
        ReturnLoanStatus Status,
        LoanResponse? Loan,
        string? ErrorMessage)
    {
        public static ReturnLoanResult Success(LoanResponse loan)
        {
            return new ReturnLoanResult(ReturnLoanStatus.Succeeded, loan, null);
        }

        public static ReturnLoanResult Failure(ReturnLoanStatus status, string message)
        {
            return new ReturnLoanResult(status, null, message);
        }
    }
}
