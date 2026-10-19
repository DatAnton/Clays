namespace Clays.Configuration;

public sealed class HangfireJobSchedules
{
    public const string SectionName = "Hangfire:JobSchedules";

    public required string KiertonetScraper { get; init; }
    public required string CompanyScraper { get; init; }
    public required string EarthScraper { get; init; }
    public required string EarthAuth { get; init; }
}
