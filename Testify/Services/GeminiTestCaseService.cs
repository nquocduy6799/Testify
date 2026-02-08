using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using Testify.Configuration;
using Testify.Interfaces;
using Testify.Shared.DTOs;
using Testify.Shared.DTOs.AI;
using Testify.Shared.Enums;

namespace Testify.Services
{
    public class GeminiTestCaseService : IAiTestCaseService
    {
        private readonly HttpClient _httpClient;
        private readonly GeminiSettings _settings;
        private readonly ILogger<GeminiTestCaseService> _logger;

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
        };

        public GeminiTestCaseService(
            HttpClient httpClient,
            IOptions<GeminiSettings> settings,
            ILogger<GeminiTestCaseService> logger)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task<AiGenerateTestCasesResponse> GenerateTestCasesAsync(AiGenerateTestCasesRequest request)
        {
            try
            {
                var prompt = BuildPrompt(request);
                var geminiResponse = await CallGeminiApi(prompt);

                if (geminiResponse == null)
                {
                    return new AiGenerateTestCasesResponse
                    {
                        Success = false,
                        Error = "Failed to get response from Gemini API."
                    };
                }

                var testCases = ParseTestCases(geminiResponse, request.DefaultPriority);

                return new AiGenerateTestCasesResponse
                {
                    Success = true,
                    TestCases = testCases
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating test cases with Gemini");
                return new AiGenerateTestCasesResponse
                {
                    Success = false,
                    Error = $"AI generation failed: {ex.Message}"
                };
            }
        }

        private string BuildPrompt(AiGenerateTestCasesRequest request)
        {
            var sb = new StringBuilder();
            sb.AppendLine("You are a QA engineer. Generate test cases in JSON format.");
            sb.AppendLine();
            sb.AppendLine("IMPORTANT: Return ONLY a valid JSON array, no markdown, no code blocks, no explanation.");
            sb.AppendLine();

            if (!string.IsNullOrWhiteSpace(request.SuiteName))
                sb.AppendLine($"Test Suite: {request.SuiteName}");

            if (!string.IsNullOrWhiteSpace(request.SuiteDescription))
                sb.AppendLine($"Suite Description: {request.SuiteDescription}");

            sb.AppendLine();
            sb.AppendLine($"Feature/Module to test: {request.Prompt}");
            sb.AppendLine($"Number of test cases to generate: {request.Count}");
            sb.AppendLine($"Default priority: {request.DefaultPriority}");

            if (request.ExistingTestCaseTitles?.Any() == true)
            {
                sb.AppendLine();
                sb.AppendLine("Existing test cases (avoid duplicating these):");
                foreach (var title in request.ExistingTestCaseTitles)
                    sb.AppendLine($"  - {title}");
            }

            sb.AppendLine();
            sb.AppendLine("Each test case must follow this exact JSON structure:");
            sb.AppendLine(@"[
  {
    ""title"": ""Test case title"",
    ""preconditions"": ""Preconditions or null"",
    ""postconditions"": ""Postconditions or null"",
    ""priority"": ""High"" | ""Medium"" | ""Low"",
    ""steps"": [
      {
        ""stepNumber"": 1,
        ""action"": ""What to do"",
        ""testData"": ""Input data or null"",
        ""expectedResult"": ""Expected outcome""
      }
    ]
  }
]");
            sb.AppendLine();
            sb.AppendLine("Requirements:");
            sb.AppendLine("- Each test case should have 2-6 clear steps");
            sb.AppendLine("- Steps should be specific and actionable");
            sb.AppendLine("- Include both positive and negative test scenarios");
            sb.AppendLine("- Cover edge cases where appropriate");
            sb.AppendLine("- Write in concise, professional QA language");

            return sb.ToString();
        }

        private async Task<string?> CallGeminiApi(string prompt)
        {
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{_settings.Model}:generateContent?key={_settings.ApiKey}";

            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                },
                generationConfig = new
                {
                    temperature = 0.7,
                    maxOutputTokens = 8192,
                    responseMimeType = "application/json"
                }
            };

            var json = JsonSerializer.Serialize(requestBody);

            _logger.LogInformation("Calling Gemini API with model: {Model}", _settings.Model);

            // Retry with exponential backoff for rate limiting (429)
            const int maxRetries = 3;
            for (int attempt = 0; attempt <= maxRetries; attempt++)
            {
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(url, content);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                {
                    if (attempt < maxRetries)
                    {
                        var delay = (int)Math.Pow(2, attempt + 1) * 1000; // 2s, 4s, 8s
                        _logger.LogWarning("Gemini rate limited (429). Retrying in {Delay}ms (attempt {Attempt}/{Max})", 
                            delay, attempt + 1, maxRetries);
                        await Task.Delay(delay);
                        continue;
                    }
                    
                    _logger.LogError("Gemini rate limit exceeded after {MaxRetries} retries", maxRetries);
                    throw new Exception("Gemini API rate limit exceeded. Please wait a moment and try again.");
                }

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Gemini API error: {StatusCode} - {Body}", response.StatusCode, responseBody);
                    throw new Exception($"Gemini API returned {response.StatusCode}");
                }

                // Parse Gemini response structure
                using var doc = JsonDocument.Parse(responseBody);
                var root = doc.RootElement;

                if (root.TryGetProperty("candidates", out var candidates) &&
                    candidates.GetArrayLength() > 0)
                {
                    var firstCandidate = candidates[0];
                    if (firstCandidate.TryGetProperty("content", out var contentElement) &&
                        contentElement.TryGetProperty("parts", out var parts) &&
                        parts.GetArrayLength() > 0)
                    {
                        return parts[0].GetProperty("text").GetString();
                    }
                }

                _logger.LogWarning("Unexpected Gemini response structure: {Body}", responseBody);
                return null;
            }

            return null;
        }

        private List<TestCaseFormResult> ParseTestCases(string jsonText, TestCasePriority defaultPriority)
        {
            // Clean up potential markdown code blocks
            jsonText = jsonText.Trim();
            if (jsonText.StartsWith("```json"))
                jsonText = jsonText[7..];
            if (jsonText.StartsWith("```"))
                jsonText = jsonText[3..];
            if (jsonText.EndsWith("```"))
                jsonText = jsonText[..^3];
            jsonText = jsonText.Trim();

            var rawCases = JsonSerializer.Deserialize<List<GeminiTestCase>>(jsonText, _jsonOptions);

            if (rawCases == null || rawCases.Count == 0)
                throw new Exception("AI returned empty or invalid test cases.");

            return rawCases.Select(raw => new TestCaseFormResult
            {
                Title = raw.Title ?? "Untitled Test Case",
                Preconditions = raw.Preconditions,
                Postconditions = raw.Postconditions,
                Priority = ParsePriority(raw.Priority, defaultPriority),
                Steps = raw.Steps?.Select((s, i) => new TestStepFormResult
                {
                    StepNumber = s.StepNumber > 0 ? s.StepNumber : i + 1,
                    Action = s.Action ?? "Step action",
                    TestData = s.TestData,
                    ExpectedResult = s.ExpectedResult ?? "Expected result"
                }).ToList() ?? new List<TestStepFormResult>()
            }).ToList();
        }

        private static TestCasePriority ParsePriority(string? priority, TestCasePriority defaultPriority)
        {
            if (string.IsNullOrWhiteSpace(priority)) return defaultPriority;
            return priority.Trim().ToLower() switch
            {
                "high" => TestCasePriority.High,
                "medium" => TestCasePriority.Medium,
                "low" => TestCasePriority.Low,
                _ => defaultPriority
            };
        }

        // Internal model to deserialize Gemini's output
        private class GeminiTestCase
        {
            public string? Title { get; set; }
            public string? Preconditions { get; set; }
            public string? Postconditions { get; set; }
            public string? Priority { get; set; }
            public List<GeminiTestStep>? Steps { get; set; }
        }

        private class GeminiTestStep
        {
            public int StepNumber { get; set; }
            public string? Action { get; set; }
            public string? TestData { get; set; }
            public string? ExpectedResult { get; set; }
        }
    }
}
