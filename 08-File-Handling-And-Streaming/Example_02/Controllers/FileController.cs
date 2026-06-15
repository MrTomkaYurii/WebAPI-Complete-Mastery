using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Example_02.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileController : ControllerBase
    {
        private readonly ILogger<FileController> _logger;

        public FileController(ILogger<FileController> logger)
        {
            _logger = logger;
        }

        [HttpPost("UploadImage01")]
        public ActionResult UploadImage01()
        {
            IFormFile file = HttpContext.Request.Form.Files[0];
            _logger.LogInformation(file.FileName);
            // we can put rest of upload logic here.
            return Ok();
        }

        [HttpPost("UploadImage02")]
        public ActionResult UploadImage02(IFormFile file)
        {

            _logger.LogInformation(file.FileName);
            // we can put rest of upload logic here.
            return Ok();
        }

        [HttpPost("UploadMultipeImages")]
        public async Task<ActionResult> UploadMultipeImages(IFormFile[] files)
        {
            foreach (var item in files)
            {
                _logger.LogInformation("file uploaded : " + item.FileName);
            }
            // we can rest of upload logic here.
            return Ok();
        }

        [HttpPost("UploadMultipeImagesWithUploadFilter")]
        public async Task<ActionResult> UploadMultipeImagesWithUploadFilter(IFormFile[] files)
        {
            foreach (var item in HttpContext.Request.Form.Files)
            {
                _logger.LogInformation("file uploaded : " + item.FileName);
            }
            // we can rest of upload logic here.
            return Ok();
        }


    }
}
