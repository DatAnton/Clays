using Microsoft.AspNetCore.Mvc;

namespace Clays.Services
{
    public sealed class ExceptionLoggingMiddleware(RequestDelegate next, ILogger<ExceptionLoggingMiddleware> logger)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (UserManagementException exception) when (!context.Response.HasStarted)
            {
                context.Response.Clear();
                context.Response.StatusCode = exception.StatusCode;
                await context.Response.WriteAsJsonAsync(new ProblemDetails
                {
                    Status = exception.StatusCode,
                    Title = exception.Message
                }, context.RequestAborted);
            }
            catch (Exception exception)
            {
                await ExceptionLogWriter.TryWriteAsync(
                    context.RequestServices,
                    exception,
                    $"HTTP {context.Request.Method} {context.Request.Path}");

                logger.LogError(exception, "Unhandled exception while processing {Method} {Path}",
                    context.Request.Method, context.Request.Path);

                if (context.Response.HasStarted)
                {
                    throw;
                }

                context.Response.Clear();
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsJsonAsync(new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "An unexpected error occurred."
                }, CancellationToken.None);
            }
        }
    }
}
