using library_system.Dtos;

namespace library_system.Services
{
    public interface IBookCommandService
    {
        Task<BookCreateResult> CreateBookAsync(
            CreateBookRequest request,
            CancellationToken cancellationToken = default);

        Task<BookUpdateResult> UpdateBookAsync(
            int id,
            CreateBookRequest request,
            CancellationToken cancellationToken = default);

        Task<BookDeleteResult> DeleteBookAsync(
            int id,
            CancellationToken cancellationToken = default);
    }
}
