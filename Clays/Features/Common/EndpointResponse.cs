using Microsoft.AspNetCore.Http;

namespace Clays.Features.Common;

internal static class EndpointResponse
{
    public static async Task JsonAsync<T>(HttpContext context, T response, int statusCode, CancellationToken cancellationToken)
    {
        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsJsonAsync(response, cancellationToken);
    }
}
