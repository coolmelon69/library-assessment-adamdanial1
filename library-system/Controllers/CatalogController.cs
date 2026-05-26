using library_system.Services;
using library_system.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace library_system.Controllers
{
    [Route("catalog")]
    public class CatalogController : Controller
    {
        private readonly IBookQueryService _bookQueryService;

        public CatalogController(IBookQueryService bookQueryService)
        {
            _bookQueryService = bookQueryService;
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
    }
}
