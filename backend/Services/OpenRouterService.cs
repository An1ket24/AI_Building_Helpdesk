using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using backend.Dtos;

namespace backend.Services;

public class OpenRouterService(HttpClient httpClient, IConfiguration configuration) : IOpenRouterService
{
    private const string Prompt = "Analyze the user message and return:\n- issue\n- category (IT, HVAC, Cleaning, Plumbing, etc.)\n- location (if any)\n- priority (Low, Medium, High)\n- solution (basic suggestion)\n\nReturn JSON only.";

    public async Task<ChatAnalysisDto> AnalyzeIssueAsync(string message, CancellationToken cancellationToken)
    {
        var apiKey = Environment.GetEnvironmentVariable("OPENROUTER_API_KEY") ?? configuration["OpenRouter:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return BuildFallbackAnalysis(message);
        }

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, configuration["OpenRouter:Url"]);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            request.Headers.Add("HTTP-Referer", configuration["Frontend:PublicUrl"] ?? "http://localhost:4200");
            request.Headers.Add("X-Title", "Smart Building Helpdesk");

            var payload = new
            {
                model = configuration["OpenRouter:Model"],
                temperature = 0.2,
                messages = new object[]
                {
                    new { role = "system", content = Prompt },
                    new { role = "user", content = message }
                }
            };

            request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            using var response = await httpClient.SendAsync(request, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            response.EnsureSuccessStatusCode();

            using var outerJson = JsonDocument.Parse(body);
            var content = outerJson.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            if (string.IsNullOrWhiteSpace(content))
            {
                return BuildFallbackAnalysis(message);
            }

            var cleanedJson = content.Trim().Trim('`');
            if (cleanedJson.StartsWith("json", StringComparison.OrdinalIgnoreCase))
            {
                cleanedJson = cleanedJson[4..].Trim();
            }

            var aiResponse = JsonSerializer.Deserialize<AiIssueResponse>(cleanedJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (aiResponse is null)
            {
                return BuildFallbackAnalysis(message);
            }

            return MapAnalysis(message, aiResponse);
        }
        catch
        {
            return BuildFallbackAnalysis(message);
        }
    }

    private static ChatAnalysisDto BuildFallbackAnalysis(string message)
    {
        var lowerMessage = message.ToLowerInvariant();
        var category = DetectCategory(lowerMessage);
        var priority = DetectPriority(lowerMessage);
        var location = DetectLocation(message);
        var issue = message.Trim();
        var solution = category switch
        {
            "IT" => "Try reconnecting to the network, restarting the affected device, or checking whether nearby users have the same issue.",
            "HVAC" => "Check the thermostat settings and nearby power supply, then confirm whether the issue affects one room or the whole floor.",
            "Plumbing" => "If water is leaking, avoid using the area and isolate the source if possible until maintenance arrives.",
            "Cleaning" => "Please keep the area clear and mark the issue so the cleaning team can address it safely.",
            _ => "Try the basic troubleshooting step for the affected equipment and confirm whether the problem continues."
        };

        var aiResponse = new AiIssueResponse
        {
            Issue = issue,
            Category = category,
            Location = location,
            Priority = priority,
            Solution = solution
        };

        return MapAnalysis(message, aiResponse);
    }

    private static ChatAnalysisDto MapAnalysis(string message, AiIssueResponse aiResponse)
    {
        var shouldOfferTicket = ShouldOfferTicket(message, aiResponse);
        var botMessage = shouldOfferTicket
            ? $"{aiResponse.Solution} Do you want me to create a ticket?"
            : aiResponse.Solution;

        return new ChatAnalysisDto(
            aiResponse.Issue,
            aiResponse.Category,
            aiResponse.Location,
            aiResponse.Priority,
            aiResponse.Solution,
            shouldOfferTicket,
            botMessage);
    }

    private static string DetectCategory(string lowerMessage)
    {
        if (lowerMessage.Contains("wifi") || lowerMessage.Contains("network") || lowerMessage.Contains("internet") || lowerMessage.Contains("computer") || lowerMessage.Contains("printer"))
        {
            return "IT";
        }

        if (lowerMessage.Contains("ac") || lowerMessage.Contains("air") || lowerMessage.Contains("cooling") || lowerMessage.Contains("heating") || lowerMessage.Contains("hvac"))
        {
            return "HVAC";
        }

        if (lowerMessage.Contains("leak") || lowerMessage.Contains("pipe") || lowerMessage.Contains("toilet") || lowerMessage.Contains("water") || lowerMessage.Contains("sink"))
        {
            return "Plumbing";
        }

        if (lowerMessage.Contains("dirty") || lowerMessage.Contains("trash") || lowerMessage.Contains("spill") || lowerMessage.Contains("clean"))
        {
            return "Cleaning";
        }

        return "General";
    }

    private static string DetectPriority(string lowerMessage)
    {
        if (lowerMessage.Contains("leak") || lowerMessage.Contains("alarm") || lowerMessage.Contains("power") || lowerMessage.Contains("urgent") || lowerMessage.Contains("smell"))
        {
            return "High";
        }

        if (lowerMessage.Contains("not working") || lowerMessage.Contains("broken") || lowerMessage.Contains("down") || lowerMessage.Contains("offline"))
        {
            return "Medium";
        }

        return "Low";
    }

    private static string? DetectLocation(string message)
    {
        var markers = new[] { "floor", "room", "building", "office", "level", "hall", "block" };
        var parts = message.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        for (var index = 0; index < parts.Length; index++)
        {
            var token = parts[index].Trim(',', '.', ';', ':').ToLowerInvariant();
            if (!markers.Contains(token))
            {
                continue;
            }

            var start = Math.Max(0, index - 1);
            var end = Math.Min(parts.Length - 1, index + 2);
            return string.Join(' ', parts[start..(end + 1)]);
        }

        return null;
    }

    private static bool ShouldOfferTicket(string message, AiIssueResponse aiResponse)
    {
        var issueText = $"{message} {aiResponse.Issue}".ToLowerInvariant();
        var urgentKeywords = new[]
        {
            "not working", "down", "offline", "broken", "leak", "water", "power", "smell", "alarm",
            "failure", "fault", "urgent", "cannot", "can't", "stopped"
        };

        if (aiResponse.Priority.Equals("High", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (urgentKeywords.Any(issueText.Contains))
        {
            return true;
        }

        return !aiResponse.Priority.Equals("Low", StringComparison.OrdinalIgnoreCase)
            || !string.IsNullOrWhiteSpace(aiResponse.Location);
    }

    private sealed class AiIssueResponse
    {
        public string Issue { get; set; } = string.Empty;
        public string Category { get; set; } = "General";
        public string? Location { get; set; }
        public string Priority { get; set; } = "Medium";
        public string Solution { get; set; } = "Please try the basic troubleshooting steps and contact support if the issue continues.";
    }
}
