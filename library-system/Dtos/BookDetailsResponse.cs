namespace library_system.Dtos
{
    public record BookDetailsResponse(
        int Id,
        string Title,
        string Author,
        string ISBN,
        int PublishedYear,
        int TotalCopies,
        int AvailableCopies);
}
