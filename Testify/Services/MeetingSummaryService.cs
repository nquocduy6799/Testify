using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Testify.Configuration;
using Testify.Interfaces;

namespace Testify.Services
{
    public class MeetingSummaryService : IMeetingSummaryService
    {
        private readonly HttpClient _httpClient;
        private readonly GeminiSettings _settings;
        private readonly IMeetingRepository _meetingRepo;
        private readonly ILogger<MeetingSummaryService> _logger;

        public MeetingSummaryService(
            HttpClient httpClient,
            IOptions<GeminiSettings> settings,
            IMeetingRepository meetingRepo,
            ILogger<MeetingSummaryService> logger)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
            _meetingRepo = meetingRepo;
            _logger = logger;
        }

        public async Task<string> GenerateSummaryAsync(int meetingId)
        {
            var meeting = await _meetingRepo.GetMeetingByIdAsync(meetingId)
                ?? throw new InvalidOperationException("Meeting not found.");

            var transcripts = await _meetingRepo.GetTranscriptsAsync(meetingId);

            // Always attempt Gemini — works with chat-only, voice-only, mixed, or even empty
            var prompt = BuildPrompt(meeting, transcripts);
            var summary = await CallGeminiApi(prompt);

            if (string.IsNullOrEmpty(summary))
            {
                summary = "Failed to generate summary. Please try again later.";
            }
            else if (summary.StartsWith("[API_ERROR:"))
            {
                var code = summary;
                summary = $"Failed to generate summary. Please try again later. ({code})";
            }

            await _meetingRepo.SaveSummaryAsync(meetingId, summary);

            _logger.LogInformation("Generated summary for meeting {MeetingId}", meetingId);
            return summary;
        }

        private static string BuildPrompt(
            Shared.DTOs.Meetings.MeetingResponse meeting,
            List<Shared.DTOs.Meetings.MeetingTranscriptEntry> transcripts)
        {
            var chatMessages = transcripts.Where(t => t.Content.StartsWith("[Chat] ")).ToList();
            var voiceMessages = transcripts.Where(t => !t.Content.StartsWith("[Chat] ")).ToList();

            var sb = new StringBuilder();
            sb.AppendLine("You are an AI meeting assistant. Your task is to read the meeting transcript and chat messages below, then produce a clear, structured meeting minutes document in the SAME LANGUAGE as the transcript (Vietnamese if the content is in Vietnamese, English if in English).");
            sb.AppendLine();
            sb.AppendLine("IMPORTANT: Return the summary in Markdown format with the following sections:");
            sb.AppendLine("## Tóm tắt (Summary)");
            sb.AppendLine("A brief 2-4 sentence overview of what was discussed.");
            sb.AppendLine();
            sb.AppendLine("## Quyết định chính (Key Decisions)");
            sb.AppendLine("Bullet list of decisions made. Write 'Không có quyết định rõ ràng' if none.");
            sb.AppendLine();
            sb.AppendLine("## Việc cần làm (Action Items)");
            sb.AppendLine("Bullet list of tasks/actions with owner if mentioned. Write 'Không có' if none.");
            sb.AppendLine();
            sb.AppendLine("## Ghi chú thảo luận (Discussion Notes)");
            sb.AppendLine("Key points raised during the discussion.");
            sb.AppendLine();
            sb.AppendLine("---");
            sb.AppendLine($"**Meeting Title:** {meeting.Title}");
            sb.AppendLine($"**Project:** {meeting.ProjectName}");
            sb.AppendLine($"**Started:** {meeting.StartedAt?.ToString("yyyy-MM-dd HH:mm")}");
            sb.AppendLine($"**Ended:** {meeting.EndedAt?.ToString("yyyy-MM-dd HH:mm")}");
            sb.AppendLine($"**Participants:** {string.Join(", ", meeting.Participants.Where(p => p.HasAttended).Select(p => p.FullName ?? p.UserName))}");
            sb.AppendLine();

            if (!transcripts.Any())
            {
                sb.AppendLine("**Note:** No transcript or chat messages were recorded during this meeting.");
                sb.AppendLine("Please generate a brief summary based on the meeting metadata above.");
            }
            else
            {
                if (voiceMessages.Any())
                {
                    sb.AppendLine("**Voice Transcript:**");
                    sb.AppendLine();
                    foreach (var t in voiceMessages)
                        sb.AppendLine($"[{t.Timestamp:HH:mm}] 🎙 **{t.UserName}**: {t.Content}");
                    sb.AppendLine();
                }

                if (chatMessages.Any())
                {
                    sb.AppendLine("**Chat Messages:**");
                    sb.AppendLine();
                    foreach (var t in chatMessages)
                        sb.AppendLine($"[{t.Timestamp:HH:mm}] 💬 **{t.UserName}**: {t.Content["[Chat] ".Length..]}");
                    sb.AppendLine();
                }
            }

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
                    temperature = 0.3,
                    maxOutputTokens = 8192
                }
            };

            var json = JsonSerializer.Serialize(requestBody);

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
                        var delay = (int)Math.Pow(2, attempt + 1) * 1000;
                        _logger.LogWarning("Gemini rate limited (429). Retrying in {Delay}ms (attempt {Attempt}/{Max})",
                            delay, attempt + 1, maxRetries);
                        await Task.Delay(delay);
                        continue;
                    }

                    _logger.LogError("Gemini rate limit exceeded after {MaxRetries} retries", maxRetries);
                    return null;
                }

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Gemini API error: {StatusCode} - {Body}", response.StatusCode, responseBody);
                    return $"[API_ERROR:{(int)response.StatusCode}]";
                }

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

                return null;
            }

            return null;
        }
    }
}
