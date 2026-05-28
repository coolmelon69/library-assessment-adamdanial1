using library_system.Data;
using library_system.Dtos;
using Microsoft.EntityFrameworkCore;

namespace library_system.Services.Books
{
    public class BookQueryService : IBookQueryService
    {
        private readonly ApplicationDbContext _context;

        public BookQueryService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<BookListItemResponse>> GetBooksAsync(
            string? title,
            string? author,
            CancellationToken cancellationToken = default)
        {
            var query = _context.Books.AsNoTracking();

            var titleFilter = NormalizeFilter(title);
            if (titleFilter is not null)
            {
                query = query.Where(book => book.Title.ToLower().Contains(titleFilter));
            }

            var authorFilter = NormalizeFilter(author);
            if (authorFilter is not null)
            {
                query = query.Where(book => book.Author.ToLower().Contains(authorFilter));
            }

            return await query
                .OrderBy(book => book.Title)
                .ThenBy(book => book.Author)
                .Select(book => new BookListItemResponse(
                    book.Id,
                    book.Title,
                    book.Author,
                    book.ISBN,
                    book.PublishedYear,
                    book.TotalCopies,
                    book.TotalCopies - book.Loans.Count(loan => loan.ReturnedDate == null)))
                .ToListAsync(cancellationToken);
        }

        public async Task<BookDetailsResponse?> GetBookByIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            return await _context.Books
                .AsNoTracking()
                .Where(book => book.Id == id)
                .Select(book => new BookDetailsResponse(
                    book.Id,
                    book.Title,
                    book.Author,
                    book.ISBN,
                    book.PublishedYear,
                    book.TotalCopies,
                    book.TotalCopies - book.Loans.Count(loan => loan.ReturnedDate == null)))
                .SingleOrDefaultAsync(cancellationToken);
        }

        private static string? NormalizeFilter(string? value)
        {
            var trimmed = value?.Trim();
            return string.IsNullOrWhiteSpace(trimmed)
                ? null
                : trimmed.ToLowerInvariant();
        }
    }
}
