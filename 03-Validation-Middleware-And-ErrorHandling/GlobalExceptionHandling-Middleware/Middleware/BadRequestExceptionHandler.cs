using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace AspNet_GlobalExceptionHandling_Middleware.Middleware;

public class BadRequestExceptionHandler : IExceptionHandler
{
    private readonly ILogger<BadRequestExceptionHandler> _logger;

    public BadRequestExceptionHandler(ILogger<BadRequestExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not BadHttpRequestException badHttpRequestException)
        {
            return false;
        }

        _logger.LogError(
            badHttpRequestException,
            "Exception occurred: {Message}",
            badHttpRequestException.Message);

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Bad Request, buddy! You need to fix it!",
            Detail = badHttpRequestException.Message,
            Type = exception.GetType().Name
        };

        httpContext.Response.StatusCode = problemDetails.Status.Value;

        await httpContext.Response
            .WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
