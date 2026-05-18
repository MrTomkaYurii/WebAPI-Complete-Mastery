using AspNet_GlobalExceptionHandling_Middleware.Models;
using AspNet_GlobalExceptionHandling_Middleware.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AspNet_GlobalExceptionHandling_Middleware.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly ILogger<BooksController> _logger;
        private ILibraryService _libraryService;

        public BooksController(
            ILibraryService libraryService,
            ILogger<BooksController> logger)
        {
            _libraryService = libraryService;
            _logger = logger;
        }

        /// <summary>
        /// Получение всех книг
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetAllBooks", Name = "GetAllBooks")]
        public ActionResult<IEnumerable<Book>> GetBooks()
        {
            return Ok(_libraryService.GetAllBooks().ToList());
        }

        /// <summary>
        /// Получение книг по автору
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        [HttpGet("GetBooksByAuthor", Name = "GetBooksByAuthor")]
        public ActionResult<Book> GetBooksByAuthor()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Получение книги по ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="BadHttpRequestException"></exception>
        [HttpGet("GetById", Name = "GetBookById")]
        public ActionResult<Book> GetBookById(int id)
        {
            var book = _libraryService.GetById(id);
            if (book is null)
            {
                throw new BadHttpRequestException($"Book with Id {id} not found, buddy!", StatusCodes.Status400BadRequest);
            }
            return Ok(book);
        }
    }
}
