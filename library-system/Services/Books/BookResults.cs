using library_system.Dtos;

namespace library_system.Services.Books
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

    public enum BookDeleteStatus
    {
        Succeeded,
        NotFound,
        HasLoanHistory
    }

    public record BookDeleteResult(
        BookDeleteStatus Status,
        string? ErrorMessage)
    {
        public static BookDeleteResult Success()
        {
            return new BookDeleteResult(BookDeleteStatus.Succeeded, null);
        }

        public static BookDeleteResult NotFound(int id)
        {
            return new BookDeleteResult(BookDeleteStatus.NotFound, $"Book with ID {id} was not found.");
        }

        public static BookDeleteResult HasLoanHistory()
        {
            return new BookDeleteResult(BookDeleteStatus.HasLoanHistory, "Books with loan history cannot be deleted.");
        }
    }
}
