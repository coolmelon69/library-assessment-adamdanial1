using System.Security.Claims;

namespace library_system.Services
{
    public interface ILoanService
    {
        Task<BorrowBookResult> BorrowBookAsync(
            int bookId,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default);
    }
}
