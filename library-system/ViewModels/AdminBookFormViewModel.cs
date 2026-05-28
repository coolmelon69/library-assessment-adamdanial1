using library_system.Dtos;

namespace library_system.ViewModels
{
    public class AdminBookFormViewModel : CreateBookRequest
    {
        public int Id { get; set; }

        public static AdminBookFormViewModel FromBook(BookDetailsResponse book)
        {
            return new AdminBookFormViewModel
            {
                Id = book.Id,
                Title = book.Title,
                Author = book.Author,
                ISBN = book.ISBN,
                PublishedYear = book.PublishedYear,
                TotalCopies = book.TotalCopies
            };
        }
    }
}
