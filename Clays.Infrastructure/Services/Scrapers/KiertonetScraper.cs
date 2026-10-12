using Clays.Application.Common.Interfaces.Scrapers;
using Clays.Application.Managers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Clays.Infrastructure.Services.Scrapers
{
    public class KiertonetScraper : IKiertonetScraper
    {
        private const string TargetUrl = "https://kiertonet.fi/filter-auctions?page=1&jarjestys=paattyvat&alue=&haku=&kategoria=9&piilota_paattyneet=0&vain_varmat_kaupat=0&is_iframe=0";
        private static readonly JsonSerializerOptions JsonSerializerOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            NumberHandling = JsonNumberHandling.AllowReadingFromString
        };

        public async Task Scrape()
        {
            using var httpClient = new HttpClient();
            using var response = await httpClient.GetAsync(TargetUrl);
            response.EnsureSuccessStatusCode();

            await using var contentStream = await response.Content.ReadAsStreamAsync();
            var payload = await JsonSerializer.DeserializeAsync<KiertonetResponse>(contentStream, JsonSerializerOptions);

            if (payload?.Data is null)
            {
                throw new InvalidOperationException("Kiertonet response did not contain a data array.");
            }

            foreach (var kiertonetAuction in payload.Data)
            {
                if (kiertonetAuction.FullTitle is { } title && MachineManager.IsWantedMachine(title))
                {
                    // send notification
                    Console.WriteLine($"Wanted machine found: {title}");
                }
            }

            Console.WriteLine(payload);
        }

        private sealed class KiertonetResponse
        {
            [JsonPropertyName("data")]
            public List<KiertonetAuction>? Data { get; init; }
        }

        private sealed class KiertonetAuction
        {
            [JsonPropertyName("id")]
            public int Id { get; init; }

            [JsonPropertyName("url")]
            public string? Url { get; init; }

            [JsonPropertyName("title")]
            public string? Title { get; init; }

            [JsonPropertyName("full_title")]
            public string? FullTitle { get; init; }

            [JsonPropertyName("ends_at")]
            public DateTimeOffset EndsAt { get; init; }

            [JsonPropertyName("fullUrl")]
            public string? FullUrl { get; init; }

            [JsonPropertyName("highest_bid")]
            public decimal HighestBid { get; init; }
        }
    }
}
