using Clays.Application.Common.Interfaces;
using Clays.Application.Common.Interfaces.Scrapers;
using Clays.Configuration;
using Hangfire;
using Microsoft.Extensions.Options;

namespace Clays.Services;

public sealed class ScopedProcessingService(
    IRecurringJobManager recurringJobManager,
    IOptions<HangfireJobSchedules> schedules) : IScopedProcessingService
{
    public void AddOrUpdateRecurringJobs()
    {
        recurringJobManager.AddOrUpdate<IKiertonetScraper>(
            "kiertonet-scraper",
            scraper => scraper.Scrape(),
            schedules.Value.KiertonetScraper);
    }
}
