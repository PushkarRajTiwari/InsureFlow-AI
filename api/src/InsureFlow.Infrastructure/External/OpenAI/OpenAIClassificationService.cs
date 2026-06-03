using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using InsureFlow.Application.Common;
using InsureFlow.Application.Interfaces;
using InsureFlow.Domain.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace InsureFlow.Infrastructure.External.OpenAI;

public sealed class OpenAIClassificationService(HttpClient httpClient, IConfiguration configuration, ILogger<OpenAIClassificationService> logger) : IAIClassificationService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<EmailClassificationResult> ClassifyAsync(string subject, string body)
    {
        logger.LogInformation("OpenAI classification call started for subject {Subject}", subject);

        var apiKey = configuration["OpenAI:ApiKey"] ?? throw new InvalidOperationException("OpenAI:ApiKey is required.");
        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/responses");
        request.Headers.Authorization = new("Bearer", apiKey);
        request.Content = JsonContent.Create(new
        {
            model = configuration["OpenAI:Model"] ?? "gpt-5.4-mini",
            input = new object[]
            {
                new
                {
                    role = "system",
                    content = "Classify insurance agency customer emails. Return only JSON with category, confidenceScore, and reasoning. Categories: Policy Change, Claim, Billing, Coverage Question, Renewal, Proof Of Insurance, General Inquiry, Spam."
                },
                new
                {
                    role = "user",
                    content = $"Subject: {subject}\nBody preview: {body}"
                }
            },
            text = new
            {
                format = new
                {
                    type = "json_schema",
                    name = "email_classification",
                    strict = true,
                    schema = new
                    {
                        type = "object",
                        additionalProperties = false,
                        required = new[] { "category", "confidenceScore", "reasoning" },
                        properties = new
                        {
                            category = new
                            {
                                type = "string",
                                @enum = new[]
                                {
                                    "Policy Change",
                                    "Claim",
                                    "Billing",
                                    "Coverage Question",
                                    "Renewal",
                                    "Proof Of Insurance",
                                    "General Inquiry",
                                    "Spam"
                                }
                            },
                            confidenceScore = new { type = "number" },
                            reasoning = new { type = "string" }
                        }
                    }
                }
            }
        });

        using var response = await httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync();
        var outputText = ExtractOutputText(responseBody);
        var parsed = JsonSerializer.Deserialize<OpenAIClassificationResponse>(outputText, JsonOptions)
            ?? throw new InvalidOperationException("OpenAI classification JSON was empty.");

        return new EmailClassificationResult(MapCategory(parsed.Category), Math.Clamp(parsed.ConfidenceScore, 0m, 1m), parsed.Reasoning);
    }

    private static string ExtractOutputText(string responseBody)
    {
        using var doc = JsonDocument.Parse(responseBody);
        foreach (var output in doc.RootElement.GetProperty("output").EnumerateArray())
        {
            if (!output.TryGetProperty("content", out var content))
            {
                continue;
            }

            foreach (var item in content.EnumerateArray())
            {
                if (item.TryGetProperty("text", out var text))
                {
                    return text.GetString() ?? "{}";
                }
            }
        }

        throw new InvalidOperationException("OpenAI response did not contain output text.");
    }

    private static EmailCategory MapCategory(string category) => category.Trim().ToLowerInvariant() switch
    {
        "policy change" => EmailCategory.PolicyChange,
        "claim" => EmailCategory.Claim,
        "billing" => EmailCategory.Billing,
        "coverage question" => EmailCategory.CoverageQuestion,
        "renewal" => EmailCategory.Renewal,
        "proof of insurance" => EmailCategory.ProofOfInsurance,
        "spam" => EmailCategory.Spam,
        _ => EmailCategory.GeneralInquiry
    };
}

internal sealed record OpenAIClassificationResponse(
    [property: JsonPropertyName("category")] string Category,
    [property: JsonPropertyName("confidenceScore")] decimal ConfidenceScore,
    [property: JsonPropertyName("reasoning")] string Reasoning);
