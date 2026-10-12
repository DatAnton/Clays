namespace Clays.Configuration;

public sealed class HangfireJobSchedules
{
    public const string SectionName = "Hangfire:JobSchedules";

    public required string KiertonetScraper { get; init; }
}
