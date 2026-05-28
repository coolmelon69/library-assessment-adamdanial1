using library_system.Dtos;

namespace library_system.Services.Books
{
    public interface IBookQueryService
    {
        Task<IReadOnlyList<BookListItemResponse>> GetBooksAsync(
            string? title,
            string? author,
            CancellationToken cancellationToken = default);

        Task<BookDetailsResponse?> GetBookByIdAsync(
            int id,
            CancellationToken cancellationToken = default);
    }
}
