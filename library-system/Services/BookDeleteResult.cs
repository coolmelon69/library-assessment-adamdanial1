namespace library_system.Services
{
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
