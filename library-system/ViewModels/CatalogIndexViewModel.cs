using library_system.Dtos;

namespace library_system.ViewModels
{
    public class CatalogIndexViewModel
    {
        public string? Title { get; set; }

        public string? Author { get; set; }

        public IReadOnlyList<BookListItemResponse> Books { get; set; } = Array.Empty<BookListItemResponse>();
    }
}
