using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;

namespace InsureFlow.Infrastructure.External.Google;

public sealed class GoogleOAuthService(HttpClient httpClient, IConfiguration configuration)
{
    public async Task<GoogleTokenResponse> ExchangeCodeAsync(string code, CancellationToken cancellationToken)
    {
        var values = new Dictionary<string, string>
        {
            ["code"] = code,
            ["client_id"] = configuration["Google:ClientId"] ?? string.Empty,
            ["client_secret"] = configuration["Google:ClientSecret"] ?? string.Empty,
            ["redirect_uri"] = configuration["Google:RedirectUri"] ?? string.Empty,
            ["grant_type"] = "authorization_code"
        };

        using var response = await httpClient.PostAsync("https://oauth2.googleapis.com/token", new FormUrlEncodedContent(values), cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<GoogleTokenResponse>(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Google token response was empty.");
    }

    public async Task<GoogleProfile> GetProfileAsync(string accessToken, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://www.googleapis.com/oauth2/v2/userinfo");
        request.Headers.Authorization = new("Bearer", accessToken);
        using var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<GoogleProfile>(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Google profile response was empty.");
    }

    public async Task<GoogleTokenResponse> RefreshAccessTokenAsync(string refreshToken, CancellationToken cancellationToken)
    {
        var values = new Dictionary<string, string>
        {
            ["refresh_token"] = refreshToken,
            ["client_id"] = configuration["Google:ClientId"] ?? string.Empty,
            ["client_secret"] = configuration["Google:ClientSecret"] ?? string.Empty,
            ["grant_type"] = "refresh_token"
        };

        using var response = await httpClient.PostAsync("https://oauth2.googleapis.com/token", new FormUrlEncodedContent(values), cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<GoogleTokenResponse>(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Google refresh response was empty.");
    }
}

public sealed record GoogleTokenResponse(
    [property: JsonPropertyName("access_token")] string AccessToken,
    [property: JsonPropertyName("refresh_token")] string? RefreshToken,
    [property: JsonPropertyName("id_token")] string? IdToken,
    [property: JsonPropertyName("expires_in")] int ExpiresIn);

public sealed record GoogleProfile(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("email")] string Email,
    [property: JsonPropertyName("name")] string Name);
