namespace Clays.Application.Common.Configuration;

public sealed class CompanyScraperConfiguration
{
    public const string SectionName = "CompanyScraperConfiguration";

    public string EndpointUrl { get; set; } = string.Empty;
}