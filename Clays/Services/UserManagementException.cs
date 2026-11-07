namespace Clays.Services;

public sealed class UserManagementException(int statusCode, string message) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
}
