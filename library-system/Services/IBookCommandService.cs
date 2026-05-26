using library_system.Dtos;

namespace library_system.Services
{
    public interface IBookCommandService
    {
        Task<BookCreateResult> CreateBookAsync(
            CreateBookRequest request,
            CancellationToken cancellationToken = default);
    }
}
