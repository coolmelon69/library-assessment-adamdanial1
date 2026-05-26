using System.Security.Claims;
using library_system.Data;
using library_system.Dtos;
using library_system.Models;
using Microsoft.EntityFrameworkCore;

namespace library_system.Services
{
    public class LoanService : ILoanService
    {
        private const int MaxActiveLoans = 3;

        private readonly ApplicationDbContext _context;
        private readonly IMemberProvisioningService _memberProvisioningService;

        public LoanService(
            ApplicationDbContext context,
            IMemberProvisioningService memberProvisioningService)
        {
            _context = context;
            _memberProvisioningService = memberProvisioningService;
        }

        public async Task<BorrowBookResult> BorrowBookAsync(
            int bookId,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var memberResult = await _memberProvisioningService.GetOrProvisionMemberAsync(user, cancellationToken);
            if (!memberResult.Succeeded || memberResult.Member is null)
            {
                return BorrowBookResult.Failure(
                    BorrowBookStatus.MissingMemberClaims,
                    memberResult.ErrorMessage ?? "Authenticated member could not be resolved.");
            }

            var book = await _context.Books
                .SingleOrDefaultAsync(existingBook => existingBook.Id == bookId, cancellationToken);

            if (book is null)
            {
                return BorrowBookResult.Failure(
                    BorrowBookStatus.BookNotFound,
                    $"Book with ID {bookId} was not found.");
            }

            var activeLoanCount = await _context.Loans
                .CountAsync(loan => loan.MemberId == memberResult.Member.Id && loan.ReturnedDate == null, cancellationToken);

            if (activeLoanCount >= MaxActiveLoans)
            {
                return BorrowBookResult.Failure(
                    BorrowBookStatus.ActiveLoanLimitReached,
                    "Members cannot have more than 3 active loans.");
            }

            var activeBookLoans = await _context.Loans
                .CountAsync(loan => loan.BookId == bookId && loan.ReturnedDate == null, cancellationToken);

            if (book.TotalCopies - activeBookLoans <= 0)
            {
                return BorrowBookResult.Failure(
                    BorrowBookStatus.NoCopiesAvailable,
                    "No copies are currently available for this book.");
            }

            var loan = new Loan
            {
                BookId = book.Id,
                MemberId = memberResult.Member.Id,
                BorrowedDate = DateTime.UtcNow,
                ReturnedDate = null
            };

            _context.Loans.Add(loan);
            await _context.SaveChangesAsync(cancellationToken);

            return BorrowBookResult.Success(new LoanResponse(
                loan.Id,
                book.Id,
                book.Title,
                book.Author,
                memberResult.Member.Id,
                loan.BorrowedDate,
                loan.ReturnedDate));
        }
    }
}
