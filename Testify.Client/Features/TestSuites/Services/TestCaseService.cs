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
                response.EnsureSuccessStatusCode();

                var created = await response.Content.ReadFromJsonAsync<TestCaseResponse>();
                return created ?? throw new InvalidOperationException("Failed to create test case.");
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
                return response.IsSuccessStatusCode;
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
