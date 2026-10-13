using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Clays.Application.Common.Configuration;
using Clays.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Clays.Infrastructure.Services;

public class EarthAuthService(
    HttpClient httpClient,
    IApplicationDbContext context,
    IOptions<EarthConfiguration> earthConfiguration, 
    ITelegramBotService telegramBotService) : IEarthAuthService
{
    private readonly EarthConfiguration _earthConfiguration = earthConfiguration.Value;

    public async Task UpdateAuthToken()
    {
        var users = await context.VinSearchSessions
            .Where(session => !session.IsCompleted)
            .Select(session => session.User)
            .Distinct()
            .ToListAsync();

        foreach (var user in users)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, _earthConfiguration.AuthUrl);
            request.Headers.TryAddWithoutValidation("grant_type", "refresh_token");
            request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "refresh_token",
                ["refresh_token"] = user.TCRefreshToken,
                ["client_id"] = _earthConfiguration.ClientId,
                ["scope"] = "openid profile offline_access"
            });

            using var response = await httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>();
            if (string.IsNullOrWhiteSpace(tokenResponse?.AccessToken)
                || string.IsNullOrWhiteSpace(tokenResponse.RefreshToken))
            {
                await telegramBotService.SendMessage(user.ChatId, "Failed to update authentication token.");
                throw new InvalidDataException("The Earth auth response did not contain both access_token and refresh_token.");
            }

            user.TCToken = tokenResponse.AccessToken;
            user.TCRefreshToken = tokenResponse.RefreshToken;

            // Persist each rotated refresh token before attempting the next user.
            await context.SaveChangesAsync(CancellationToken.None);
        }
    }

    private sealed class TokenResponse
    {
        [JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }

        [JsonPropertyName("refresh_token")]
        public string? RefreshToken { get; set; }
    }
}
