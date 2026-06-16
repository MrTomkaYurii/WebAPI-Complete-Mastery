using UploadingLargeFiles.DTO;

namespace Services;

public interface IFileService
{
    Task<FileUploadSummary> UploadFileAsync(Stream fileStream, string contentType);
}
