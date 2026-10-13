using System.Globalization;
using System.Net.Http.Headers;
using System.Numerics;
using Clays.Application.Common.Configuration;
using Clays.Application.Common.Interfaces;
using Clays.Application.Common.Interfaces.Scrapers;
using Clays.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Clays.Infrastructure.Services.Scrapers;

public class EarthScraper(
    HttpClient httpClient,
    IApplicationDbContext context,
    IOptions<EarthConfiguration> earthConfiguration,
    ITelegramBotService telegramBotService) : IEarthScraper
{
    private readonly EarthConfiguration _earthConfiguration = earthConfiguration.Value;

    public async Task Scrape()
    {
        var sessions = await context.VinSearchSessions
            .Include(session => session.User)
            .Include(session => session.VinEntries)
            .Where(session => !session.IsCompleted)
            .ToListAsync();

        foreach (var session in sessions)
        {
            if (string.IsNullOrWhiteSpace(session.User.TCToken))
            {
                throw new InvalidOperationException($"User {session.UserId} has no Earth access token.");
            }

            await telegramBotService.SendMessage(
                session.User.ChatId,
                $"Starting VIN search for session {session.Id}.",
                CancellationToken.None);

            var (startPrefix, startNumber, startWidth) = SplitVin(session.VinStart);
            var (endPrefix, endNumber, _) = SplitVin(session.VinEnd);
            if (startPrefix != endPrefix || startNumber > endNumber)
            {
                throw new InvalidOperationException($"Invalid VIN range for session {session.Id}.");
            }

            var nextNumber = startNumber;
            if (session.VinEntries.Count > 0)
            {
                var lastEntry = session.VinEntries.OrderBy(entry => entry.Id).Last();
                var (lastPrefix, lastNumber, _) = SplitVin(lastEntry.VinCode);
                if (lastPrefix != startPrefix)
                {
                    throw new InvalidOperationException($"VIN entry {lastEntry.Id} does not match session {session.Id}.");
                }

                nextNumber = lastNumber + BigInteger.One;
            }

            for (; nextNumber <= endNumber; nextNumber++)
            {
                var vin = $"{startPrefix}{nextNumber.ToString($"D{startWidth}", CultureInfo.InvariantCulture)}";

                await telegramBotService.SendMessage(
                    session.User.ChatId,
                    $"Checking vin:{vin}.",
                    CancellationToken.None);

                using var request = new HttpRequestMessage(HttpMethod.Get, CreateVinUri(vin));
                request.Headers.Add("Cookie", _earthConfiguration.Cookie);
                request.Headers.Add("Host", _earthConfiguration.Host);
                request.Headers.Add("Referer", _earthConfiguration.Referer);
                request.Headers.Add("Authorization", $"Bearer {session.User.TCToken}");

                using var response = await httpClient.SendAsync(request);
                if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                {
                    await telegramBotService.SendMessage(
                        session.User.ChatId,
                        $"Session {session.Id} is rate-limited.",
                        CancellationToken.None);
                    return;
                }
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    await telegramBotService.SendMessage(
                        session.User.ChatId,
                        $"Session {session.Id} is unauthorized. Please refresh the token.",
                        CancellationToken.None);
                    return;
                }

                var isFound = response.StatusCode switch
                {
                    System.Net.HttpStatusCode.OK => true,
                    System.Net.HttpStatusCode.NotFound => false,
                    _ => throw new HttpRequestException(
                        $"Unexpected HTTP status {(int)response.StatusCode} while checking VIN {vin}.",
                        null,
                        response.StatusCode)
                };

                context.VinEntries.Add(new VinEntry
                {
                    VinCode = vin,
                    IsFound = isFound,
                    VinSearchSessionId = session.Id
                });
                await context.SaveChangesAsync(CancellationToken.None);

                if (isFound)
                {
                    await telegramBotService.SendMessage(
                        session.User.ChatId,
                        $"VIN found: {vin} (session {session.Id}).",
                        CancellationToken.None);

                    if (nextNumber < endNumber)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(Random.Shared.Next(10, 51)));
                    }
                }
            }

            await telegramBotService.SendMessage(
                        session.User.ChatId,
                        $"Session {session.Id} completed.",
                        CancellationToken.None);

            session.IsCompleted = true;
            await context.SaveChangesAsync(CancellationToken.None);
        }
    }

    private Uri CreateVinUri(string vin)
    {
        var url = _earthConfiguration.Url.Replace(
            "{vin}",
            Uri.EscapeDataString(vin),
            StringComparison.OrdinalIgnoreCase);
        return new Uri(url, UriKind.Absolute);
    }

    private static (string Prefix, BigInteger Number, int Width) SplitVin(string vin)
    {
        var suffixStart = vin.Length;
        while (suffixStart > 0 && char.IsAsciiDigit(vin[suffixStart - 1]))
        {
            suffixStart--;
        }

        if (suffixStart == vin.Length
            || !BigInteger.TryParse(vin.AsSpan(suffixStart), NumberStyles.None, CultureInfo.InvariantCulture, out var number))
        {
            throw new InvalidOperationException($"VIN '{vin}' must end with a numeric sequence.");
        }

        return (vin[..suffixStart], number, vin.Length - suffixStart);
    }
}
