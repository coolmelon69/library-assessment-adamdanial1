using library_system.Data;
using library_system.Dtos;
using library_system.Models;
using Microsoft.EntityFrameworkCore;

namespace library_system.Services
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
    }
}
