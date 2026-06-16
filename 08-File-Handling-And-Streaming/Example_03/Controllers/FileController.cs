using Microsoft.AspNetCore.Mvc;
using Services;
using Utilities;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FileController : ControllerBase
    {
        private readonly IFileService _fileService;

        public FileController(IFileService fileService)
        {
            _fileService = fileService;
        }


        // custom action filter attribute MultipartFormData which ensures that the incoming
        // request has the correct content type of multipart/form-data.Of course, we call the
        // DisableFormValueModelBinding filter, to disable model validation.

        [HttpPost("upload-stream-multipartreader")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status415UnsupportedMediaType)]
        [MultipartFormData]
        [DisableFormValueModelBinding]
        public async Task<IActionResult> Upload()
        {
            var fileUploadSummary = await _fileService.UploadFileAsync(HttpContext.Request.Body, Request.ContentType);

            return CreatedAtAction(nameof(Upload), fileUploadSummary);
        }
    }
}