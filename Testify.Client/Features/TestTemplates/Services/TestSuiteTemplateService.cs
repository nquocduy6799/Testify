using System.Net.Http.Json;
using Testify.Client.Interfaces;
using Testify.Shared.DTOs.TestTemplates;

namespace Testify.Client.Features.TestTemplates.Services
{
    public class TestSuiteTemplateService : ITestSuiteTemplateService
    {
        private readonly HttpClient _httpClient;
        private const string ApiEndpoint = "api/testsuitetemplates";

        public TestSuiteTemplateService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<TestSuiteTemplateResponse>> GetTestSuiteTemplatesAsync()
        {
            try
            {
                var templates = await _httpClient.GetFromJsonAsync<List<TestSuiteTemplateResponse>>(ApiEndpoint);
                return templates ?? new List<TestSuiteTemplateResponse>();
            }
            catch (HttpRequestException)
            {
                return new List<TestSuiteTemplateResponse>();
            }
        }

        public async Task<List<TestSuiteTemplateResponse>> GetCloneableTemplatesAsync()
        {
            try
            {
                var templates = await _httpClient.GetFromJsonAsync<List<TestSuiteTemplateResponse>>($"{ApiEndpoint}/cloneable");
                return templates ?? new List<TestSuiteTemplateResponse>();
            }
            catch (HttpRequestException)
            {
                return new List<TestSuiteTemplateResponse>();
            }
        }

        public async Task<TestSuiteTemplateResponse> GetTestSuiteTemplateByIdAsync(int id)
        {
            try
            {
                var template = await _httpClient.GetFromJsonAsync<TestSuiteTemplateResponse>($"{ApiEndpoint}/{id}");
                return template ?? throw new InvalidOperationException($"Test suite template with ID {id} not found.");
            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException($"Failed to retrieve test suite template with ID {id}.", ex);
            }
        }

        public async Task<TestSuiteTemplateResponse> CreateTestSuiteTemplateAsync(CreateTestSuiteTemplateRequest request)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync(ApiEndpoint, request);
                response.EnsureSuccessStatusCode();

                var createdTemplate = await response.Content.ReadFromJsonAsync<TestSuiteTemplateResponse>();
                return createdTemplate ?? throw new InvalidOperationException("Failed to create test suite template.");
            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException("Failed to create test suite template.", ex);
            }
        }

        public async Task<TestSuiteTemplateResponse> UpdateTestSuiteTemplateAsync(int id, UpdateTestSuiteTemplateRequest request)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"{ApiEndpoint}/{id}", request);
                response.EnsureSuccessStatusCode();

                // Since PUT returns NoContent (204), fetch the updated template
                return await GetTestSuiteTemplateByIdAsync(id);
            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException($"Failed to update test suite template with ID {id}.", ex);
            }
        }

        public async Task<bool> DeleteTestSuiteTemplateAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{ApiEndpoint}/{id}");
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException)
            {
                return false;
            }
        }
    }
}