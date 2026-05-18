using AspNet_GlobalExceptionHandling_Middleware.Models;

namespace AspNet_GlobalExceptionHandling_Middleware.Services;

public interface ILibraryService
{
    Book? GetById(int id);
    List<Book> GetAllBooks();
}
