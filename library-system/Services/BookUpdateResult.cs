using library_system.Dtos;

namespace library_system.Services
{
    public enum BookUpdateStatus
    {
        Succeeded,
        NotFound,
        DuplicateIsbn,
        CopiesBelowActiveLoans
    }

    public record BookUpdateResult(
        BookUpdateStatus Status,
        BookDetailsResponse? Book,
        string? ErrorMessage)
    {
        public static BookUpdateResult Success(BookDetailsResponse book)
        {
            return new BookUpdateResult(BookUpdateStatus.Succeeded, book, null);
        }

        public static BookUpdateResult NotFound(int id)
        {
            return new BookUpdateResult(BookUpdateStatus.NotFound, null, $"Book with ID {id} was not found.");
        }

        public static BookUpdateResult DuplicateIsbn(string isbn)
        {
            return new BookUpdateResult(BookUpdateStatus.DuplicateIsbn, null, $"A book with ISBN '{isbn}' already exists.");
        }

        public static BookUpdateResult CopiesBelowActiveLoans(int activeLoans)
        {
            return new BookUpdateResult(
                BookUpdateStatus.CopiesBelowActiveLoans,
                null,
                $"Total copies cannot be less than the {activeLoans} active loan(s) for this book.");
        }
    }
}
