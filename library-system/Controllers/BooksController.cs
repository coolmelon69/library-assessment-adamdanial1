using library_system.Dtos;
using library_system.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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
        private readonly ILoanService _loanService;

        public BooksController(
            IBookQueryService bookQueryService,
            IBookCommandService bookCommandService,
            ILoanService loanService)
        {
            _bookQueryService = bookQueryService;
            _bookCommandService = bookCommandService;
            _loanService = loanService;
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

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
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

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("{bookId:int}/borrow")]
        public async Task<ActionResult<LoanResponse>> BorrowBook(
            int bookId,
            CancellationToken cancellationToken)
        {
            var result = await _loanService.BorrowBookAsync(bookId, User, cancellationToken);

            return result.Status switch
            {
                BorrowBookStatus.Succeeded => Created($"/loans/{result.Loan!.Id}", result.Loan),
                BorrowBookStatus.BookNotFound => NotFound(new { message = result.ErrorMessage }),
                BorrowBookStatus.MissingMemberClaims => BadRequest(new { message = result.ErrorMessage }),
                BorrowBookStatus.NoCopiesAvailable => Conflict(new { message = result.ErrorMessage }),
                BorrowBookStatus.ActiveLoanLimitReached => Conflict(new { message = result.ErrorMessage }),
                _ => BadRequest(new { message = "The borrow request could not be completed." })
            };
        }
    }
}
