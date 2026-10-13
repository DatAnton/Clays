namespace Clays.Domain.Entities
{
    public class ErrorLog
    {
        public int Id { get; set; }
        public DateTimeOffset OccurredAtUtc { get; set; }
        public string ExceptionType { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? StackTrace { get; set; }
        public string Source { get; set; } = string.Empty;
    }
}