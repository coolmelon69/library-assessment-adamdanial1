using System.Security.Claims;
using library_system.Data;
using library_system.Dtos;
using library_system.Models;
using library_system.Services.Members;
using Microsoft.EntityFrameworkCore;

namespace library_system.Services.Loans
{
    public class LoanService : ILoanService
    {
        private const int MaxActiveLoans = 3;

        private readonly ApplicationDbContext _context;
        private readonly IMemberProvisioningService _memberProvisioningService;
        private readonly ILogger<LoanService> _logger;

        public LoanService(
            ApplicationDbContext context,
            IMemberProvisioningService memberProvisioningService,
            ILogger<LoanService> logger)
        {
            _context = context;
            _memberProvisioningService = memberProvisioningService;
            _logger = logger;
        }

        public async Task<BorrowBookResult> BorrowBookAsync(
            int bookId,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var memberResult = await _memberProvisioningService.GetOrProvisionMemberAsync(user, cancellationToken);
            if (!memberResult.Succeeded || memberResult.Member is null)
            {
                _logger.LogWarning(
                    "Borrow request for book {BookId} was rejected because the authenticated member could not be resolved.",
                    bookId);

                return BorrowBookResult.Failure(
                    BorrowBookStatus.MissingMemberClaims,
                    memberResult.ErrorMessage ?? "Authenticated member could not be resolved.");
            }

            var book = await _context.Books
                .SingleOrDefaultAsync(existingBook => existingBook.Id == bookId, cancellationToken);

            if (book is null)
            {
                _logger.LogWarning(
                    "Borrow request for book {BookId} by member {MemberId} was rejected because the book was not found.",
                    bookId,
                    memberResult.Member.Id);

                return BorrowBookResult.Failure(
                    BorrowBookStatus.BookNotFound,
                    $"Book with ID {bookId} was not found.");
            }

            var activeLoanCount = await _context.Loans
                .CountAsync(loan => loan.MemberId == memberResult.Member.Id && loan.ReturnedDate == null, cancellationToken);

            if (activeLoanCount >= MaxActiveLoans)
            {
                _logger.LogWarning(
                    "Borrow request for book {BookId} by member {MemberId} was rejected because the member has {ActiveLoanCount} active loans.",
                    bookId,
                    memberResult.Member.Id,
                    activeLoanCount);

                return BorrowBookResult.Failure(
                    BorrowBookStatus.ActiveLoanLimitReached,
                    "Members cannot have more than 3 active loans.");
            }

            var activeBookLoans = await _context.Loans
                .CountAsync(loan => loan.BookId == bookId && loan.ReturnedDate == null, cancellationToken);

            if (book.TotalCopies - activeBookLoans <= 0)
            {
                _logger.LogWarning(
                    "Borrow request for book {BookId} by member {MemberId} was rejected because all {TotalCopies} copies are active loans.",
                    bookId,
                    memberResult.Member.Id,
                    book.TotalCopies);

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

            _logger.LogInformation(
                "Created loan {LoanId} for book {BookId} and member {MemberId}.",
                loan.Id,
                book.Id,
                memberResult.Member.Id);

            return BorrowBookResult.Success(new LoanResponse(
                loan.Id,
                book.Id,
                book.Title,
                book.Author,
                memberResult.Member.Id,
                loan.BorrowedDate,
                loan.ReturnedDate));
        }

        public async Task<IReadOnlyList<LoanResponse>> GetActiveLoansForCurrentMemberAsync(
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var memberResult = await _memberProvisioningService.GetOrProvisionMemberAsync(user, cancellationToken);
            if (!memberResult.Succeeded || memberResult.Member is null)
            {
                _logger.LogWarning(
                    "Active loan lookup was rejected because the authenticated member could not be resolved.");

                return Array.Empty<LoanResponse>();
            }

            return await _context.Loans
                .AsNoTracking()
                .Where(loan => loan.MemberId == memberResult.Member.Id && loan.ReturnedDate == null)
                .OrderByDescending(loan => loan.BorrowedDate)
                .Select(loan => new LoanResponse(
                    loan.Id,
                    loan.BookId,
                    loan.Book.Title,
                    loan.Book.Author,
                    loan.MemberId,
                    loan.BorrowedDate,
                    loan.ReturnedDate))
                .ToListAsync(cancellationToken);
        }

        public async Task<ReturnLoanResult> ReturnLoanAsync(
            int loanId,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var memberResult = await _memberProvisioningService.GetOrProvisionMemberAsync(user, cancellationToken);
            if (!memberResult.Succeeded || memberResult.Member is null)
            {
                _logger.LogWarning(
                    "Return request for loan {LoanId} was rejected because the authenticated member could not be resolved.",
                    loanId);

                return ReturnLoanResult.Failure(
                    ReturnLoanStatus.MissingMemberClaims,
                    memberResult.ErrorMessage ?? "Authenticated member could not be resolved.");
            }

            var loan = await _context.Loans
                .Include(existingLoan => existingLoan.Book)
                .SingleOrDefaultAsync(existingLoan => existingLoan.Id == loanId, cancellationToken);

            if (loan is null)
            {
                _logger.LogWarning(
                    "Return request for loan {LoanId} by member {MemberId} was rejected because the loan was not found.",
                    loanId,
                    memberResult.Member.Id);

                return ReturnLoanResult.Failure(
                    ReturnLoanStatus.LoanNotFound,
                    $"Loan with ID {loanId} was not found.");
            }

            if (loan.MemberId != memberResult.Member.Id)
            {
                _logger.LogWarning(
                    "Return request for loan {LoanId} by member {MemberId} was rejected because the loan belongs to member {LoanMemberId}.",
                    loanId,
                    memberResult.Member.Id,
                    loan.MemberId);

                return ReturnLoanResult.Failure(
                    ReturnLoanStatus.Forbidden,
                    "Members can only return their own loans.");
            }

            if (loan.ReturnedDate is not null)
            {
                _logger.LogWarning(
                    "Return request for loan {LoanId} by member {MemberId} was rejected because the loan was already returned.",
                    loanId,
                    memberResult.Member.Id);

                return ReturnLoanResult.Failure(
                    ReturnLoanStatus.AlreadyReturned,
                    "This loan has already been returned.");
            }

            loan.ReturnedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Returned loan {LoanId} for book {BookId} and member {MemberId}.",
                loan.Id,
                loan.BookId,
                memberResult.Member.Id);

            return ReturnLoanResult.Success(new LoanResponse(
                loan.Id,
                loan.BookId,
                loan.Book.Title,
                loan.Book.Author,
                loan.MemberId,
                loan.BorrowedDate,
                loan.ReturnedDate));
        }
    }
}
