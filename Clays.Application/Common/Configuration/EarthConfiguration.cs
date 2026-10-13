namespace Clays.Application.Common.Configuration;

public sealed class EarthConfiguration
{
    public const string SectionName = "EarthConfiguration";
    public string AuthUrl { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Cookie { get; set; } = string.Empty;
    public string Host { get; set; } = string.Empty;
    public string Referer { get; set; } = string.Empty;
}