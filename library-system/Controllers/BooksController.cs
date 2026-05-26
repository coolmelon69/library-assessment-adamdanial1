using library_system.Dtos;
using library_system.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace library_system.Controllers
{
    [ApiController]
    [Route("books")]
    public class BooksController : ControllerBase
    {
        private readonly IBookQueryService _bookQueryService;
        private readonly IBookCommandService _bookCommandService;

        public BooksController(
            IBookQueryService bookQueryService,
            IBookCommandService bookCommandService)
        {
            _bookQueryService = bookQueryService;
            _bookCommandService = bookCommandService;
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

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<BookDetailsResponse>> CreateBook(
            CreateBookRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _bookCommandService.CreateBookAsync(request, cancellationToken);

            if (!result.Succeeded)
            {
                return Conflict(new { message = result.ErrorMessage });
            }

            return CreatedAtAction(
                nameof(GetBook),
                new { id = result.Book!.Id },
                result.Book);
        }
    }
}
