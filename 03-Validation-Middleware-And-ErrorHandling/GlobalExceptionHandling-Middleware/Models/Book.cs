using Microsoft.AspNetCore.OpenApi;
using Microsoft.AspNetCore.Http.HttpResults;
namespace AspNet_GlobalExceptionHandling_Middleware.Models;

public class Book
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public required string Author { get; set; }
}
