using System.Net.Http.Json;
using Testify.Client.Interfaces;
using Testify.Shared.DTOs.TestSuites;

namespace Testify.Client.Features.TestSuites.Services
{
    public class TestCaseService : ITestCaseService
    {
        private readonly HttpClient _httpClient;

        public TestCaseService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<TestCaseResponse> GetTestCaseByIdAsync(int id)
        {
            try
            {
                var testCase = await _httpClient.GetFromJsonAsync<TestCaseResponse>($"api/testcases/{id}");
                return testCase ?? throw new InvalidOperationException($"Test case with ID {id} not found.");
            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException($"Failed to retrieve test case with ID {id}.", ex);
            }
        }

        public async Task<TestCaseResponse> CreateTestCaseAsync(int suiteId, CreateTestCaseRequest request)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"api/testsuites/{suiteId}/testcases", request);

                if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
                {
                    var error = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
                    var message = error?.GetValueOrDefault("message") ?? $"A test case named \"{request.Title}\" already exists in this suite.";
                    throw new InvalidOperationException(message);
                }

                response.EnsureSuccessStatusCode();

                var created = await response.Content.ReadFromJsonAsync<TestCaseResponse>();
                return created ?? throw new InvalidOperationException("Failed to create test case.");
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException("Failed to create test case.", ex);
            }
        }

        public async Task<bool> UpdateTestCaseAsync(int id, UpdateTestCaseRequest request)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"api/testcases/{id}", request);

                if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
                {
                    var error = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
                    var message = error?.GetValueOrDefault("message") ?? $"A test case named \"{request.Title}\" already exists in this suite.";
                    throw new InvalidOperationException(message);
                }

                return response.IsSuccessStatusCode;
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (HttpRequestException)
            {
                return false;
            }
        }

        public async Task<bool> DeleteTestCaseAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/testcases/{id}");
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException)
            {
                return false;
            }
        }
    }
}
