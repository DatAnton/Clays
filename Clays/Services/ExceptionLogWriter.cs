using Clays.Domain.Entities;
using Clays.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Clays.Services
{
    public static class ExceptionLogWriter
    {
        public static async Task TryWriteAsync(
            IServiceProvider services,
            Exception exception,
            string source)
        {
            try
            {
                await using var scope = services.CreateAsyncScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                dbContext.ErrorLogs.Add(new ErrorLog
                {
                    OccurredAtUtc = DateTimeOffset.UtcNow,
                    ExceptionType = exception.GetType().FullName ?? exception.GetType().Name,
                    Message = exception.Message,
                    StackTrace = exception.ToString(),
                    Source = source
                });

                await dbContext.SaveChangesAsync(CancellationToken.None);
            }
            catch (Exception loggingException)
            {
                var logger = services.GetService<ILoggerFactory>()?.CreateLogger("ExceptionLogWriter");
                logger?.LogError(loggingException,
                    "Failed to persist an exception to the Logs table. Original exception: {OriginalException}",
                    exception);
            }
        }
    }
}