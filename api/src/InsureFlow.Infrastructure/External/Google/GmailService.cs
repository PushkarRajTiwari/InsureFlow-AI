using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Serialization;
using InsureFlow.Application.Common;
using InsureFlow.Application.Interfaces;

namespace InsureFlow.Infrastructure.External.Google;

public sealed class GmailService(HttpClient httpClient) : IGmailService
{
    public async Task<IReadOnlyList<ImportedEmail>> GetLatestEmailsAsync(string accessToken, int maxResults, CancellationToken cancellationToken)
    {
        using var listRequest = new HttpRequestMessage(HttpMethod.Get, $"https://gmail.googleapis.com/gmail/v1/users/me/messages?maxResults={maxResults}");
        listRequest.Headers.Authorization = new("Bearer", accessToken);
        using var listResponse = await httpClient.SendAsync(listRequest, cancellationToken);
        listResponse.EnsureSuccessStatusCode();
        var list = await listResponse.Content.ReadFromJsonAsync<GmailListResponse>(cancellationToken: cancellationToken);
        var messages = new List<ImportedEmail>();

        foreach (var item in list?.Messages ?? [])
        {
            using var messageRequest = new HttpRequestMessage(HttpMethod.Get, $"https://gmail.googleapis.com/gmail/v1/users/me/messages/{item.Id}?format=metadata&metadataHeaders=From&metadataHeaders=Subject&metadataHeaders=Date");
            messageRequest.Headers.Authorization = new("Bearer", accessToken);
            using var messageResponse = await httpClient.SendAsync(messageRequest, cancellationToken);
            messageResponse.EnsureSuccessStatusCode();
            var message = await messageResponse.Content.ReadFromJsonAsync<GmailMessageResponse>(cancellationToken: cancellationToken);
            if (message is null)
            {
                continue;
            }

            var headers = message.Payload.Headers.ToDictionary(x => x.Name, x => x.Value, StringComparer.OrdinalIgnoreCase);
            var date = headers.TryGetValue("Date", out var rawDate) && DateTimeOffset.TryParse(rawDate, out var parsedDate)
                ? parsedDate
                : DateTimeOffset.FromUnixTimeMilliseconds(message.InternalDate);

            messages.Add(new ImportedEmail(
                message.Id,
                message.ThreadId,
                headers.GetValueOrDefault("From", "Unknown sender"),
                headers.GetValueOrDefault("Subject", "(no subject)"),
                DecodeSnippet(message.Snippet),
                date));
        }

        return messages;
    }

    private static string DecodeSnippet(string snippet)
    {
        return Encoding.UTF8.GetString(Encoding.UTF8.GetBytes(snippet)).Trim();
    }
}

internal sealed record GmailListResponse([property: JsonPropertyName("messages")] IReadOnlyList<GmailListItem>? Messages);
internal sealed record GmailListItem([property: JsonPropertyName("id")] string Id);
internal sealed record GmailMessageResponse(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("threadId")] string ThreadId,
    [property: JsonPropertyName("snippet")] string Snippet,
    [property: JsonPropertyName("internalDate")] long InternalDate,
    [property: JsonPropertyName("payload")] GmailPayload Payload);
internal sealed record GmailPayload([property: JsonPropertyName("headers")] IReadOnlyList<GmailHeader> Headers);
internal sealed record GmailHeader([property: JsonPropertyName("name")] string Name, [property: JsonPropertyName("value")] string Value);
