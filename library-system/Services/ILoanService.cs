using System.Security.Claims;
using library_system.Dtos;

namespace library_system.Services
{
    public interface ILoanService
    {
        Task<BorrowBookResult> BorrowBookAsync(
            int bookId,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<LoanResponse>> GetActiveLoansForCurrentMemberAsync(
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default);

        Task<ReturnLoanResult> ReturnLoanAsync(
            int loanId,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default);
    }
}
