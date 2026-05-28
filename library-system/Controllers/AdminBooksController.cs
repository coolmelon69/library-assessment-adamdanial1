using library_system.Security;
using library_system.Services.Books;
using library_system.ViewModels;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace library_system.Controllers
{
    [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme, Policy = AuthorizationPolicies.AdminOnly)]
    [Route("admin/books")]
    public class AdminBooksController : Controller
    {
        private readonly IBookQueryService _bookQueryService;
        private readonly IBookCommandService _bookCommandService;

        public AdminBooksController(
            IBookQueryService bookQueryService,
            IBookCommandService bookCommandService)
        {
            _bookQueryService = bookQueryService;
            _bookCommandService = bookCommandService;
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

            return book is null
                ? NotFound()
                : View(book);
        }

        [HttpGet("create")]
        public IActionResult Create()
        {
            return View(new AdminBookFormViewModel());
        }

        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            AdminBookFormViewModel model,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _bookCommandService.CreateBookAsync(model, cancellationToken);

            if (!result.Succeeded)
            {
                ModelState.AddModelError(nameof(model.ISBN), result.ErrorMessage ?? "The book could not be created.");
                return View(model);
            }

            TempData["AdminBooksSuccessMessage"] = "Book created successfully.";
            return RedirectToAction(nameof(Details), new { id = result.Book!.Id });
        }

        [HttpGet("{id:int}/edit")]
        public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
        {
            var book = await _bookQueryService.GetBookByIdAsync(id, cancellationToken);

            return book is null
                ? NotFound()
                : View(AdminBookFormViewModel.FromBook(book));
        }

        [HttpPost("{id:int}/edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            AdminBookFormViewModel model,
            CancellationToken cancellationToken)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _bookCommandService.UpdateBookAsync(id, model, cancellationToken);

            switch (result.Status)
            {
                case BookUpdateStatus.Succeeded:
                    TempData["AdminBooksSuccessMessage"] = "Book updated successfully.";
                    return RedirectToAction(nameof(Details), new { id });

                case BookUpdateStatus.NotFound:
                    return NotFound();

                case BookUpdateStatus.DuplicateIsbn:
                    ModelState.AddModelError(nameof(model.ISBN), result.ErrorMessage!);
                    return View(model);

                case BookUpdateStatus.CopiesBelowActiveLoans:
                    ModelState.AddModelError(nameof(model.TotalCopies), result.ErrorMessage!);
                    return View(model);

                default:
                    ModelState.AddModelError(string.Empty, "The book update could not be completed.");
                    return View(model);
            }
        }

        [HttpGet("{id:int}/delete")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var book = await _bookQueryService.GetBookByIdAsync(id, cancellationToken);

            return book is null
                ? NotFound()
                : View(book);
        }

        [HttpPost("{id:int}/delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken cancellationToken)
        {
            var result = await _bookCommandService.DeleteBookAsync(id, cancellationToken);

            switch (result.Status)
            {
                case BookDeleteStatus.Succeeded:
                    TempData["AdminBooksSuccessMessage"] = "Book deleted successfully.";
                    return RedirectToAction(nameof(Index));

                case BookDeleteStatus.NotFound:
                    return NotFound();

                case BookDeleteStatus.HasLoanHistory:
                    TempData["AdminBooksErrorMessage"] = result.ErrorMessage;
                    return RedirectToAction(nameof(Details), new { id });

                default:
                    TempData["AdminBooksErrorMessage"] = "The book delete request could not be completed.";
                    return RedirectToAction(nameof(Details), new { id });
            }
        }
    }
}
