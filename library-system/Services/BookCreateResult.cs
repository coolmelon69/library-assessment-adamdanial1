using library_system.Dtos;

namespace library_system.Services
{
    public record BookCreateResult(
        bool Succeeded,
        BookDetailsResponse? Book,
        string? ErrorMessage)
    {
        public static BookCreateResult Success(BookDetailsResponse book)
        {
            return new BookCreateResult(true, book, null);
        }

        public static BookCreateResult DuplicateIsbn(string isbn)
        {
            return new BookCreateResult(false, null, $"A book with ISBN '{isbn}' already exists.");
        }
    }
}
