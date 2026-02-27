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
        public async Task IncrementViewCountAsync(int id)
        {
            try
            {
                await _httpClient.PostAsync($"{ApiEndpoint}/{id}/view", null);
            }
            catch
            {
                // Ignore errors for view count
            }
        }

        public async Task IncrementCloneCountAsync(int id)
        {
            try
            {
                await _httpClient.PostAsync($"{ApiEndpoint}/{id}/clone", null);
            }
            catch
            {
                // Ignore errors for clone count
            }
        }

        public async Task<(int deleted, int failed)> BulkDeleteTemplatesAsync(List<int> templateIds)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"{ApiEndpoint}/bulk-delete", new { templateIds });
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<BulkOperationResult>();
                return (result?.Deleted ?? 0, result?.Failed ?? 0);
            }
            catch (HttpRequestException)
            {
                return (0, templateIds.Count);
            }
        }

        public async Task<(int moved, int failed)> BulkMoveTemplatesAsync(List<int> templateIds, int? targetFolderId)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"{ApiEndpoint}/bulk-move", new { templateIds, targetFolderId });
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<BulkOperationResult>();
                return (result?.Moved ?? 0, result?.Failed ?? 0);
            }
            catch (HttpRequestException)
            {
                return (0, templateIds.Count);
            }
        }

        private class BulkOperationResult
        {
            public int Deleted { get; set; }
            public int Failed { get; set; }
            public int Moved { get; set; }
        }
    }
}



//using System.Net.Http.Json;
//using Testify.Client.Interfaces;
//using Testify.Shared.DTOs.TestTemplates;

//namespace Testify.Client.Features.TestTemplates.Services
//{
//    public class TestSuiteTemplateService : ITestSuiteTemplateService
//    {
//        private readonly HttpClient _httpClient;
//        private const string ApiEndpoint = "api/testsuitetemplates";

//        public TestSuiteTemplateService(HttpClient httpClient)
//        {
//            _httpClient = httpClient;
//        }

//        public async Task<List<TestSuiteTemplateResponse>> GetTestSuiteTemplatesAsync()
//        {
//            try
//            {
//                var templates = await _httpClient.GetFromJsonAsync<List<TestSuiteTemplateResponse>>(ApiEndpoint);
//                return templates ?? new List<TestSuiteTemplateResponse>();
//            }
//            catch (HttpRequestException)
//            {
//                return new List<TestSuiteTemplateResponse>();
//            }
//        }

//        public async Task<TestSuiteTemplateResponse> GetTestSuiteTemplateByIdAsync(int id)
//        {
//            try
//            {
//                var template = await _httpClient.GetFromJsonAsync<TestSuiteTemplateResponse>($"{ApiEndpoint}/{id}");
//                return template ?? throw new InvalidOperationException($"Test suite template with ID {id} not found.");
//            }
//            catch (HttpRequestException ex)
//            {
//                throw new InvalidOperationException($"Failed to retrieve test suite template with ID {id}.", ex);
//            }
//        }

//        public async Task<TestSuiteTemplateResponse> CreateTestSuiteTemplateAsync(CreateTestSuiteTemplateRequest request)
//        {
//            try
//            {
//                var response = await _httpClient.PostAsJsonAsync(ApiEndpoint, request);
//                response.EnsureSuccessStatusCode();

//                var createdTemplate = await response.Content.ReadFromJsonAsync<TestSuiteTemplateResponse>();
//                return createdTemplate ?? throw new InvalidOperationException("Failed to create test suite template.");
//            }
//            catch (HttpRequestException ex)
//            {
//                throw new InvalidOperationException("Failed to create test suite template.", ex);
//            }
//        }

//        public async Task<TestSuiteTemplateResponse> UpdateTestSuiteTemplateAsync(int id, UpdateTestSuiteTemplateRequest request)
//        {
//            try
//            {
//                var response = await _httpClient.PutAsJsonAsync($"{ApiEndpoint}/{id}", request);
//                response.EnsureSuccessStatusCode();

//                // Since PUT returns NoContent (204), fetch the updated template
//                return await GetTestSuiteTemplateByIdAsync(id);
//            }
//            catch (HttpRequestException ex)
//            {
//                throw new InvalidOperationException($"Failed to update test suite template with ID {id}.", ex);
//            }
//        }

//        public async Task<bool> DeleteTestSuiteTemplateAsync(int id)
//        {
//            try
//            {
//                var response = await _httpClient.DeleteAsync($"{ApiEndpoint}/{id}");
//                return response.IsSuccessStatusCode;
//            }
//            catch (HttpRequestException)
//            {
//                return false;
//            }
//        }
//    }
//}