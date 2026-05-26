using library_system.Dtos;
using library_system.Services;
using Microsoft.AspNetCore.Mvc;

namespace library_system.Controllers
{
    [ApiController]
    [Route("books")]
    public class BooksController : ControllerBase
    {
        private readonly IBookQueryService _bookQueryService;

        public BooksController(IBookQueryService bookQueryService)
        {
            _bookQueryService = bookQueryService;
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<BookListItemResponse>>> GetBooks(
            [FromQuery] string? title,
            [FromQuery] string? author,
            CancellationToken cancellationToken)
        {
            var books = await _bookQueryService.GetBooksAsync(title, author, cancellationToken);

            return Ok(books);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<BookDetailsResponse>> GetBook(
            int id,
            CancellationToken cancellationToken)
        {
            var book = await _bookQueryService.GetBookByIdAsync(id, cancellationToken);

            if (book is null)
            {
                return NotFound(new { message = $"Book with ID {id} was not found." });
            }

            return Ok(book);
        }
    }
}
