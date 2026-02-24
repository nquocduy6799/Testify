using System.Net.Http.Json;
using Testify.Client.Interfaces;
using Testify.Shared.DTOs.AI;

namespace Testify.Client.Features.TestSuites.Services
{
    public class AiTestCaseService : IAiTestCaseService
    {
        private readonly HttpClient _http;

        public AiTestCaseService(HttpClient http)
        {
            _http = http;
        }

        public async Task<AiUsageResponse> GetUsageAsync()
        {
            try
            {
                var result = await _http.GetFromJsonAsync<AiUsageResponse>("api/AiTestCases/usage");
                return result ?? new AiUsageResponse { UsedCount = 0, MaxCount = 3 };
            }
            catch
            {
                return new AiUsageResponse { UsedCount = 0, MaxCount = 3 };
            }
        }

        public async Task<AiGenerateTestCasesResponse> GenerateAsync(AiGenerateTestCasesRequest request)
        {
            var response = await _http.PostAsJsonAsync("api/AiTestCases/generate", request);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<AiGenerateTestCasesResponse>();
                return result ?? new AiGenerateTestCasesResponse { Success = false, Error = "Empty response." };
            }

            // Try to read error response  
            try
            {
                var errorResult = await response.Content.ReadFromJsonAsync<AiGenerateTestCasesResponse>();
                return errorResult ?? new AiGenerateTestCasesResponse
                {
                    Success = false,
                    Error = $"API returned {response.StatusCode}."
                };
            }
            catch
            {
                return new AiGenerateTestCasesResponse
                {
                    Success = false,
                    Error = $"API returned {response.StatusCode}."
                };
            }
        }
    }
}
