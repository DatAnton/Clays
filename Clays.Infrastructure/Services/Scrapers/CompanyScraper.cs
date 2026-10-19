using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Clays.Application.Common.Configuration;
using Clays.Application.Common.Interfaces;
using Clays.Application.Common.Interfaces.Scrapers;
using Clays.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Clays.Infrastructure.Services.Scrapers;

public sealed class CompanyScraper(
    HttpClient httpClient,
    IApplicationDbContext context,
    ILogger<CompanyScraper> logger,
    IOptions<CompanyScraperConfiguration> configuration) : ICompanyScraper
{
    public async Task Scrape()
    {
        var page = 1;
        long totalResults;
        long processed = 0;

        do
        {
            var url = BuildPageUrl(configuration.Value.EndpointUrl, page);
            var payload = await httpClient.GetFromJsonAsync<CompanyResult>(url);
            if (payload?.Companies is null)
            {
                throw new InvalidDataException("PRH response did not contain a companies array.");
            }

            totalResults = payload.TotalResults;
            if (payload.Companies.Count == 0 && processed < totalResults)
            {
                throw new InvalidDataException("PRH returned an empty page before all companies were retrieved.");
            }

            var companies = payload.Companies
                .Where(company => !string.IsNullOrWhiteSpace(company.BusinessId?.Value))
                .GroupBy(company => company.BusinessId!.Value!, StringComparer.Ordinal)
                .Select(group => group.First())
                .ToList();

            var businessIds = companies.Select(company => company.BusinessId!.Value!).ToArray();
            var existingCompanies = await context.Companies
                .Where(company => company.BusinessId != null && businessIds.Contains(company.BusinessId))
                .ToListAsync();
            var existingByBusinessId = existingCompanies
                .GroupBy(company => company.BusinessId!, StringComparer.Ordinal)
                .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);

            foreach (var companyData in companies)
            {
                var businessId = companyData.BusinessId!.Value!;
                var name = SelectName(companyData.Names);
                if (string.IsNullOrWhiteSpace(name))
                {
                    logger.LogWarning("Skipping PRH company {BusinessId} because it has no name.", businessId);
                    continue;
                }

                var address = SelectAddress(companyData.Addresses);
                if (!existingByBusinessId.TryGetValue(businessId, out var company))
                {
                    company = new Company { BusinessId = businessId };
                    context.Companies.Add(company);
                    existingByBusinessId.Add(businessId, company);
                }

                company.Name = name;
                company.City = address.City;
                company.Address = address.Address;
                company.MainBusinessLine = SelectBusinessLine(companyData.MainBusinessLine);
            }

            await context.SaveChangesAsync(CancellationToken.None);
            processed += payload.Companies.Count;
            page++;
        }
        while (processed < totalResults);

        logger.LogInformation("PRH company scrape completed; processed {CompanyCount} results.", processed);
    }

    private static string BuildPageUrl(string endpointUrl, int page)
    {
        var endpoint = new UriBuilder(endpointUrl);
        var queryParameters = endpoint.Query
            .TrimStart('?')
            .Split('&', StringSplitOptions.RemoveEmptyEntries)
            .Where(parameter => !parameter.StartsWith("page=", StringComparison.OrdinalIgnoreCase));
        endpoint.Query = string.Join("&", queryParameters.Append($"page={page}"));
        return endpoint.Uri.ToString();
    }

    private static string? SelectName(IReadOnlyCollection<RegisterName>? names) => names?
        .OrderByDescending(name => name.Version == 1)
        .Select(name => name.Name)
        .FirstOrDefault(name => !string.IsNullOrWhiteSpace(name));

    private static (string? City, string? Address) SelectAddress(IReadOnlyCollection<CompanyAddress>? addresses)
    {
        var address = addresses?.FirstOrDefault(item => item.Type == 1)
            ?? addresses?.FirstOrDefault();
        var city = address?.PostOffices?
            .OrderByDescending(postOffice => postOffice.LanguageCode == "1")
            .Select(postOffice => postOffice.City)
            .FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));

        var street = string.Join(" ", new[] { address?.Street, address?.BuildingNumber }
            .Where(value => !string.IsNullOrWhiteSpace(value)));
        var addressParts = new[] { street, address?.PostCode }
            .Where(value => !string.IsNullOrWhiteSpace(value));
        return (city, string.Join(", ", addressParts));
    }

    private static string? SelectBusinessLine(MainBusinessLine? businessLine) =>
        businessLine?.Descriptions?
            .OrderByDescending(description => description.LanguageCode == "3")
            .Select(description => description.Description)
            .FirstOrDefault(description => !string.IsNullOrWhiteSpace(description))
        ?? businessLine?.Type;

    private sealed class CompanyResult
    {
        [JsonPropertyName("totalResults")]
        public long TotalResults { get; init; }

        [JsonPropertyName("companies")]
        public List<CompanyRecord>? Companies { get; init; }
    }

    private sealed class CompanyRecord
    {
        [JsonPropertyName("businessId")]
        public BusinessId? BusinessId { get; init; }

        [JsonPropertyName("names")]
        public List<RegisterName>? Names { get; init; }

        [JsonPropertyName("mainBusinessLine")]
        public MainBusinessLine? MainBusinessLine { get; init; }

        [JsonPropertyName("addresses")]
        public List<CompanyAddress>? Addresses { get; init; }
    }

    private sealed class BusinessId
    {
        [JsonPropertyName("value")]
        public string? Value { get; init; }
    }

    private sealed class RegisterName
    {
        [JsonPropertyName("name")]
        public string? Name { get; init; }

        [JsonPropertyName("version")]
        public int Version { get; init; }
    }

    private sealed class MainBusinessLine
    {
        [JsonPropertyName("type")]
        public string? Type { get; init; }

        [JsonPropertyName("descriptions")]
        public List<DescriptionEntry>? Descriptions { get; init; }
    }

    private sealed class DescriptionEntry
    {
        [JsonPropertyName("languageCode")]
        public string? LanguageCode { get; init; }

        [JsonPropertyName("description")]
        public string? Description { get; init; }
    }

    private sealed class CompanyAddress
    {
        [JsonPropertyName("type")]
        public int Type { get; init; }

        [JsonPropertyName("street")]
        public string? Street { get; init; }

        [JsonPropertyName("buildingNumber")]
        public string? BuildingNumber { get; init; }

        [JsonPropertyName("postCode")]
        public string? PostCode { get; init; }

        [JsonPropertyName("postOffices")]
        public List<PostOffice>? PostOffices { get; init; }
    }

    private sealed class PostOffice
    {
        [JsonPropertyName("city")]
        public string? City { get; init; }

        [JsonPropertyName("languageCode")]
        public string? LanguageCode { get; init; }
    }
}