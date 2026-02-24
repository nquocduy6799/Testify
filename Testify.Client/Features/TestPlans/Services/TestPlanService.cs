using System.Net.Http.Json;
using Testify.Client.Interfaces;
using Testify.Shared.DTOs.TestCases;
using Testify.Shared.DTOs.TestPlans;

namespace Testify.Client.Features.TestPlans.Services
{
    public class TestPlanService : ITestPlanService
    {
        private readonly HttpClient _httpClient;
        private const string ApiEndpoint = "api/testplans";

        public TestPlanService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<TestPlanResponse>> GetAllTestPlansAsync(int projectId)
        {
            try
            {
                var testPlans = await _httpClient.GetFromJsonAsync<List<TestPlanResponse>>($"{ApiEndpoint}/project/{projectId}");
                return testPlans ?? new List<TestPlanResponse>();
            }
            catch (HttpRequestException)
            {
                return new List<TestPlanResponse>();
            }
        }

        public async Task<TestPlanResponse?> GetTestPlanByIdAsync(int id)
        {
            try
            {
                var testPlan = await _httpClient.GetFromJsonAsync<TestPlanResponse>($"{ApiEndpoint}/{id}");
                return testPlan;
            }
            catch (HttpRequestException)
            {
                return null;
            }
        }

        public async Task<TestPlanResponse> CreateTestPlanAsync(CreateTestPlanRequest request)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync(ApiEndpoint, request);
                response.EnsureSuccessStatusCode();

                var createdTestPlan = await response.Content.ReadFromJsonAsync<TestPlanResponse>();
                return createdTestPlan ?? throw new InvalidOperationException("Failed to create test plan.");
            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException("Failed to create test plan.", ex);
            }
        }

        public async Task<bool> UpdateTestPlanAsync(int id, UpdateTestPlanRequest request)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"{ApiEndpoint}/{id}", request);
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException)
            {
                return false;
            }
        }

        public async Task<bool> DeleteTestPlanAsync(int id)
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

        public async Task<IEnumerable<TestSuiteResponse>> GetAllTestSuitesByProjectIdAsync(int projectId)
        {
            try
            {
                var testSuites = await _httpClient.GetFromJsonAsync<IEnumerable<TestSuiteResponse>>($"{ApiEndpoint}/project/{projectId}/testsuites");
                return testSuites ?? Enumerable.Empty<TestSuiteResponse>();
            }
            catch (HttpRequestException)
            {
                return Enumerable.Empty<TestSuiteResponse>();
            }
        }
    }
}