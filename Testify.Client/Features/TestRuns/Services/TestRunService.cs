using System.Net.Http.Json;
using Testify.Client.Interfaces;
using Testify.Shared.DTOs.TestRuns;
using Testify.Shared.Enums;

namespace Testify.Client.Features.TestRuns.Services
{
    public class TestRunService : ITestRunService
    {
        private readonly HttpClient _httpClient;
        private const string ApiEndpoint = "api/testruns";

        public TestRunService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        #region Test Run CRUD Operations

        public async Task<TestRunResponse?> GetByIdAsync(int id, bool includeSteps = false)
        {
            try
            {
                var url = $"{ApiEndpoint}/{id}?includeSteps={includeSteps}";
                var testRun = await _httpClient.GetFromJsonAsync<TestRunResponse>(url);
                return testRun;
            }
            catch (HttpRequestException)
            {
                return null;
            }
        }

        public async Task<TestRunDetailResponse?> GetDetailedByIdAsync(int id)
        {
            try
            {
                var testRun = await _httpClient.GetFromJsonAsync<TestRunDetailResponse>($"{ApiEndpoint}/{id}/detailed");
                return testRun;
            }
            catch (HttpRequestException)
            {
                return null;
            }
        }

        public async Task<List<TestRunResponse>> GetByTestPlanIdAsync(int testPlanId)
        {
            try
            {
                var testRuns = await _httpClient.GetFromJsonAsync<List<TestRunResponse>>($"{ApiEndpoint}/testplan/{testPlanId}");
                return testRuns ?? new List<TestRunResponse>();
            }
            catch (HttpRequestException)
            {
                return new List<TestRunResponse>();
            }
        }

        public async Task<List<TestRunResponse>> GetByStatusAsync(int testPlanId, TestRunStatus status)
        {
            try
            {
                var testRuns = await _httpClient.GetFromJsonAsync<List<TestRunResponse>>($"{ApiEndpoint}/testplan/{testPlanId}/status/{status}");
                return testRuns ?? new List<TestRunResponse>();
            }
            catch (HttpRequestException)
            {
                return new List<TestRunResponse>();
            }
        }

        public async Task<List<TestRunResponse>> GetByExecutedByAsync(string userId)
        {
            try
            {
                var testRuns = await _httpClient.GetFromJsonAsync<List<TestRunResponse>>($"{ApiEndpoint}/user/{userId}");
                return testRuns ?? new List<TestRunResponse>();
            }
            catch (HttpRequestException)
            {
                return new List<TestRunResponse>();
            }
        }

        public async Task<TestRunResponse> CreateAsync(CreateTestRunRequest request)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync(ApiEndpoint, request);
                response.EnsureSuccessStatusCode();

                var createdTestRun = await response.Content.ReadFromJsonAsync<TestRunResponse>();
                return createdTestRun ?? throw new InvalidOperationException("Failed to create test run.");
            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException("Failed to create test run.", ex);
            }
        }

        public async Task<bool> UpdateAsync(int id, UpdateTestRunRequest request)
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

        public async Task<bool> DeleteAsync(int id)
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

        #endregion

        #region UC-09: Start Test Plan Execution

        public async Task<StartExecutionResponse?> StartExecutionAsync(int testPlanId, List<int> testSuiteIds)
        {
            try
            {
                var request = new StartExecutionRequest
                {
                    TestPlanId = testPlanId,
                    TestSuiteIds = testSuiteIds
                };

                var response = await _httpClient.PostAsJsonAsync($"{ApiEndpoint}/start-execution", request);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<StartExecutionResponse>();
                return result;
            }
            catch (HttpRequestException ex)
            {
                // Log or handle the error appropriately
                Console.WriteLine($"Failed to start test plan execution: {ex.Message}");
                return null;
            }
        }

        public async Task<BulkCreateTestRunsResponse?> BulkCreateTestRunsAsync(int testPlanId, List<int> testCaseIds)
        {
            try
            {
                var request = new BulkCreateTestRunsRequest
                {
                    TestPlanId = testPlanId,
                    TestCaseIds = testCaseIds
                };

                var response = await _httpClient.PostAsJsonAsync($"{ApiEndpoint}/bulk-create", request);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<BulkCreateTestRunsResponse>();
                return result;
            }
            catch (HttpRequestException)
            {
                return null;
            }
        }

        #endregion

        #region Test Run Step Operations

        public async Task<List<TestRunStepResponse>> GetStepsByTestRunIdAsync(int testRunId)
        {
            try
            {
                var steps = await _httpClient.GetFromJsonAsync<List<TestRunStepResponse>>($"{ApiEndpoint}/{testRunId}/steps");
                return steps ?? new List<TestRunStepResponse>();
            }
            catch (HttpRequestException)
            {
                return new List<TestRunStepResponse>();
            }
        }

        public async Task<TestRunStepResponse?> GetStepByIdAsync(int stepId)
        {
            try
            {
                var step = await _httpClient.GetFromJsonAsync<TestRunStepResponse>($"{ApiEndpoint}/steps/{stepId}");
                return step;
            }
            catch (HttpRequestException)
            {
                return null;
            }
        }

        public async Task<bool> UpdateStepAsync(int stepId, UpdateTestRunStepRequest request)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"{ApiEndpoint}/steps/{stepId}", request);
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException)
            {
                return false;
            }
        }

        public async Task<bool> BatchUpdateStepsAsync(Dictionary<int, UpdateTestRunStepRequest> updates)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"{ApiEndpoint}/steps/batch", updates);
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException)
            {
                return false;
            }
        }

        #endregion

        #region Statistics & Reporting

        public async Task<TestRunStatistics?> GetStatisticsByTestPlanIdAsync(int testPlanId)
        {
            try
            {
                var stats = await _httpClient.GetFromJsonAsync<TestRunStatistics>($"{ApiEndpoint}/testplan/{testPlanId}/statistics");
                return stats;
            }
            catch (HttpRequestException)
            {
                return null;
            }
        }

        public async Task<bool> HasTestRunsAsync(int testPlanId)
        {
            try
            {
                var result = await _httpClient.GetFromJsonAsync<bool>($"{ApiEndpoint}/testplan/{testPlanId}/has-runs");
                return result;
            }
            catch (HttpRequestException)
            {
                return false;
            }
        }

        #endregion
    }
}