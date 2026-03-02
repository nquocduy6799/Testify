using System.Net.Http.Json;
using Testify.Client.Interfaces;
using Testify.Shared.DTOs.TestSuites;

namespace Testify.Client.Features.TestSuites.Services
{
    public class TestSuiteService : ITestSuiteService
    {
        private readonly HttpClient _httpClient;
        private const string ApiEndpoint = "api/testsuites";

        public TestSuiteService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<TestSuiteResponse>> GetTestSuitesByProjectIdAsync(int projectId)
        {
            try
            {
                var suites = await _httpClient.GetFromJsonAsync<List<TestSuiteResponse>>($"{ApiEndpoint}/project/{projectId}");
                return suites ?? new List<TestSuiteResponse>();
            }
            catch (HttpRequestException)
            {
                return new List<TestSuiteResponse>();
            }
        }

        public async Task<TestSuiteResponse> GetTestSuiteByIdAsync(int id)
        {
            try
            {
                var suite = await _httpClient.GetFromJsonAsync<TestSuiteResponse>($"{ApiEndpoint}/{id}");
                return suite ?? throw new InvalidOperationException($"Test suite with ID {id} not found.");
            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException($"Failed to retrieve test suite with ID {id}.", ex);
            }
        }

        public async Task<TestSuiteResponse> CreateTestSuiteAsync(CreateTestSuiteRequest request)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync(ApiEndpoint, request);

                if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
                {
                    var error = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
                    var message = error?.GetValueOrDefault("message") ?? "A test suite with this name already exists in this project.";
                    throw new InvalidOperationException(message);
                }

                response.EnsureSuccessStatusCode();

                var created = await response.Content.ReadFromJsonAsync<TestSuiteResponse>();
                return created ?? throw new InvalidOperationException("Failed to create test suite.");
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException("Failed to create test suite.", ex);
            }
        }

        public async Task<TestSuiteResponse> UpdateTestSuiteAsync(int id, UpdateTestSuiteRequest request)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"{ApiEndpoint}/{id}", request);
                response.EnsureSuccessStatusCode();
                return await GetTestSuiteByIdAsync(id);
            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException($"Failed to update test suite with ID {id}.", ex);
            }
        }

        public async Task<bool> DeleteTestSuiteAsync(int id)
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

        public async Task<bool> CheckSuiteNameExistsAsync(int projectId, string name, int? excludeId = null)
        {
            try
            {
                var url = $"{ApiEndpoint}/check-name?projectId={projectId}&name={Uri.EscapeDataString(name)}";
                if (excludeId.HasValue)
                    url += $"&excludeId={excludeId.Value}";
                var result = await _httpClient.GetFromJsonAsync<bool>(url);
                return result;
            }
            catch
            {
                return false;
            }
        }

        public async Task<string> SuggestUniqueNameAsync(int projectId, string baseName)
        {
            try
            {
                var url = $"{ApiEndpoint}/suggest-name?projectId={projectId}&baseName={Uri.EscapeDataString(baseName)}";
                var result = await _httpClient.GetStringAsync(url);
                return result;
            }
            catch
            {
                return baseName;
            }
        }
    }
}
