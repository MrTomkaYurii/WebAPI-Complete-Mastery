using AspNet_ModelStateValidation.Filters;
using AspNet_ModelStateValidation.Models;
using Microsoft.AspNetCore.Mvc;


namespace AspNet_ModelStateValidation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController : ControllerBase
    {

        private readonly ILogger<BooksController> _logger;

        public BooksController(ILogger<BooksController> logger)
        {
            _logger = logger;
        }


        [HttpPost("Example01", Name = "PostBooks")]
        public IActionResult Post_01([FromBody] CreateBookInputModel createBookInputModel)
        {
            return Ok(createBookInputModel);
        }

        [HttpPost("Example02", Name = "PostBooksWithValidation")]
        public IActionResult Post_02([FromBody] CreateBookInputModel createBookInputModel)
        {
            return Ok(createBookInputModel);
        }

        [HttpPost("Example03", Name = "PostBooksWithModelState")]
        public IActionResult Post_03([FromBody] CreateBookInputModel createBookInputModel)
        {
            if (!ModelState.IsValid)
            {
                return UnprocessableEntity(ModelState);
            }
            return Ok(createBookInputModel);
        }


        [HttpPost("Example04", Name = "PostBooksWithFilter")]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        public IActionResult Post_04(CreateBookInputModel createBookInputModel)
        {
            return Ok(createBookInputModel);
        }

        [HttpPost("Example05", Name = "PostBooksWithCustomValidation")]
        public IActionResult Post_05([FromBody] CreateBookInputModel createBookInputModel)
        {
            if (createBookInputModel.Title != null
                && createBookInputModel.Description != null
                && !createBookInputModel.Description.Contains(createBookInputModel.Title))
            {
                ModelState.AddModelError(nameof(createBookInputModel.Description), "Book description should contain book title!");
            }

            if (!ModelState.IsValid)
            {
                return UnprocessableEntity(ModelState);
            }

            return Ok(createBookInputModel);
        }
    }

}
