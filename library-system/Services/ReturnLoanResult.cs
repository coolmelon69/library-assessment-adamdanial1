using library_system.Dtos;

namespace library_system.Services
{
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
