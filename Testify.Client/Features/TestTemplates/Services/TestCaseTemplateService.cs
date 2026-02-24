using System.Net.Http.Json;
using Testify.Client.Interfaces;
using Testify.Shared.DTOs.TestCases;

namespace Testify.Client.Features.TestTemplates.Services
{
    public class TestCaseTemplateService : ITestCaseTemplateService
    {
        private readonly HttpClient _httpClient;
        // Endpoints
        private const string TestCaseApiEndpoint = "api/testcasetemplates";
        private const string TestSuiteApiEndpoint = "api/testsuitetemplates";

        public TestCaseTemplateService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<TestCaseTemplateResponse> GetTestCaseTemplateByIdAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<TestCaseTemplateResponse>($"{TestCaseApiEndpoint}/{id}");
                return response ?? throw new InvalidOperationException($"Test case template with ID {id} not found.");
            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException($"Failed to retrieve test case template with ID {id}.", ex);
            }
        }

        public async Task<TestCaseTemplateResponse> CreateTestCaseTemplateAsync(int suiteId, CreateTestCaseTemplateRequest request)
        {
            try
            {
                // POST to suite-centric endpoint
                var response = await _httpClient.PostAsJsonAsync($"{TestSuiteApiEndpoint}/{suiteId}/testcases", request);
                response.EnsureSuccessStatusCode();

                var createdTemplate = await response.Content.ReadFromJsonAsync<TestCaseTemplateResponse>();
                return createdTemplate ?? throw new InvalidOperationException("Failed to create test case template.");
            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException("Failed to create test case template.", ex);
            }
        }

        public async Task<TestCaseTemplateResponse> UpdateTestCaseTemplateAsync(int id, UpdateTestCaseTemplateRequest request)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"{TestCaseApiEndpoint}/{id}", request);
                response.EnsureSuccessStatusCode();
   
                return await GetTestCaseTemplateByIdAsync(id);
            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException($"Failed to update test case template with ID {id}.", ex);
            }
        }

        public async Task<bool> DeleteTestCaseTemplateAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{TestCaseApiEndpoint}/{id}");
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException)
            {
                return false;
            }
        }
    }
}
