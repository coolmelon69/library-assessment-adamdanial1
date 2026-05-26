using library_system.Services;
using library_system.ViewModels;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace library_system.Controllers
{
    [Route("catalog")]
    public class CatalogController : Controller
    {
        private readonly IBookQueryService _bookQueryService;
        private readonly ILoanService _loanService;

        public CatalogController(
            IBookQueryService bookQueryService,
            ILoanService loanService)
        {
            _bookQueryService = bookQueryService;
            _loanService = loanService;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index(
            [FromQuery] string? title,
            [FromQuery] string? author,
            CancellationToken cancellationToken)
        {
            var books = await _bookQueryService.GetBooksAsync(title, author, cancellationToken);

            return View(new CatalogIndexViewModel
            {
                Title = title,
                Author = author,
                Books = books
            });
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
        {
            var book = await _bookQueryService.GetBookByIdAsync(id, cancellationToken);

            if (book is null)
            {
                return NotFound();
            }

            return View(new CatalogDetailsViewModel
            {
                Book = book
            });
        }

        [HttpPost("{id:int}/borrow")]
        [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Borrow(int id, CancellationToken cancellationToken)
        {
            var result = await _loanService.BorrowBookAsync(id, User, cancellationToken);

            switch (result.Status)
            {
                case BorrowBookStatus.Succeeded:
                    TempData["CatalogSuccessMessage"] = "Book borrowed successfully.";
                    return RedirectToAction(nameof(Details), new { id });

                case BorrowBookStatus.BookNotFound:
                    return NotFound();

                case BorrowBookStatus.MissingMemberClaims:
                    TempData["CatalogErrorMessage"] = "Your Google sign-in could not be resolved. Please sign out and sign in again.";
                    return RedirectToAction(nameof(Details), new { id });

                case BorrowBookStatus.NoCopiesAvailable:
                case BorrowBookStatus.ActiveLoanLimitReached:
                    TempData["CatalogErrorMessage"] = result.ErrorMessage;
                    return RedirectToAction(nameof(Details), new { id });

                default:
                    TempData["CatalogErrorMessage"] = "The borrow request could not be completed.";
                    return RedirectToAction(nameof(Details), new { id });
            }
        }
    }
}
