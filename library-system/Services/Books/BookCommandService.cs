using library_system.Data;
using library_system.Dtos;
using library_system.Models;
using Microsoft.EntityFrameworkCore;

namespace library_system.Services.Books
{
    public class BookCommandService : IBookCommandService
    {
        private readonly ApplicationDbContext _context;

        public BookCommandService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<BookCreateResult> CreateBookAsync(
            CreateBookRequest request,
            CancellationToken cancellationToken = default)
        {
            var isbn = request.ISBN.Trim();
            var isbnExists = await _context.Books
                .AnyAsync(book => book.ISBN == isbn, cancellationToken);

            if (isbnExists)
            {
                return BookCreateResult.DuplicateIsbn(isbn);
            }

            var book = new Book
            {
                Title = request.Title.Trim(),
                Author = request.Author.Trim(),
                ISBN = isbn,
                PublishedYear = request.PublishedYear,
                TotalCopies = request.TotalCopies
            };

            _context.Books.Add(book);
            await _context.SaveChangesAsync(cancellationToken);

            return BookCreateResult.Success(new BookDetailsResponse(
                book.Id,
                book.Title,
                book.Author,
                book.ISBN,
                book.PublishedYear,
                book.TotalCopies,
                book.TotalCopies));
        }

        public async Task<BookUpdateResult> UpdateBookAsync(
            int id,
            CreateBookRequest request,
            CancellationToken cancellationToken = default)
        {
            var book = await _context.Books
                .Include(existingBook => existingBook.Loans)
                .SingleOrDefaultAsync(existingBook => existingBook.Id == id, cancellationToken);

            if (book is null)
            {
                return BookUpdateResult.NotFound(id);
            }

            var isbn = request.ISBN.Trim();
            var isbnExists = await _context.Books
                .AnyAsync(existingBook => existingBook.Id != id && existingBook.ISBN == isbn, cancellationToken);

            if (isbnExists)
            {
                return BookUpdateResult.DuplicateIsbn(isbn);
            }

            var activeLoans = book.Loans.Count(loan => loan.ReturnedDate == null);
            if (request.TotalCopies < activeLoans)
            {
                return BookUpdateResult.CopiesBelowActiveLoans(activeLoans);
            }

            book.Title = request.Title.Trim();
            book.Author = request.Author.Trim();
            book.ISBN = isbn;
            book.PublishedYear = request.PublishedYear;
            book.TotalCopies = request.TotalCopies;

            await _context.SaveChangesAsync(cancellationToken);

            return BookUpdateResult.Success(new BookDetailsResponse(
                book.Id,
                book.Title,
                book.Author,
                book.ISBN,
                book.PublishedYear,
                book.TotalCopies,
                book.TotalCopies - activeLoans));
        }

        public async Task<BookDeleteResult> DeleteBookAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            var book = await _context.Books
                .Include(existingBook => existingBook.Loans)
                .SingleOrDefaultAsync(existingBook => existingBook.Id == id, cancellationToken);

            if (book is null)
            {
                return BookDeleteResult.NotFound(id);
            }

            if (book.Loans.Count > 0)
            {
                return BookDeleteResult.HasLoanHistory();
            }

            _context.Books.Remove(book);
            await _context.SaveChangesAsync(cancellationToken);

            return BookDeleteResult.Success();
        }
    }
}
