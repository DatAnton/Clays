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

            recurringJobManager.AddOrUpdate<IEarthScraper>(
            "earth-scraper",
            scraper => scraper.Scrape(),
            schedules.Value.EarthScraper);

            recurringJobManager.AddOrUpdate<IEarthAuthService>(
            "earth-auth",
            authService => authService.UpdateAuthToken(),
            schedules.Value.EarthAuth);
    }
}
