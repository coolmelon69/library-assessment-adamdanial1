namespace library_system.Dtos
{
    public record BookListItemResponse(
        int Id,
        string Title,
        string Author,
        string ISBN,
        int PublishedYear,
        int TotalCopies,
        int AvailableCopies);
}
