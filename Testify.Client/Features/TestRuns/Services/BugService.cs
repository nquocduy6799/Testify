using System.Net.Http.Json;
using Testify.Client.Interfaces;
using Testify.Shared.DTOs.Bugs;

namespace Testify.Client.Features.TestRuns.Services
{
    public class BugService : IBugService
    {
        private readonly HttpClient _httpClient;
        private const string ApiEndpoint = "api/bugs";

        public BugService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        #region Bug CRUD Operations

        public async Task<BugResponse?> CreateBugFromTestRunAsync(CreateBugFromTestRunRequest request)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"{ApiEndpoint}/fromtestrun", request);
                response.EnsureSuccessStatusCode();

                var createdBug = await response.Content.ReadFromJsonAsync<BugResponse>();
                return createdBug;
            }
            catch (HttpRequestException)
            {
                return new BugResponse();
            }
        }

        public async Task<BugResponse?> GetByIdAsync(int id)
        {
            try
            {
                var bug = await _httpClient.GetFromJsonAsync<BugResponse>($"{ApiEndpoint}/{id}");
                return bug;
            }
            catch (HttpRequestException)
            {
                return new BugResponse();
            }
        }

        public async Task<List<BugResponse>> GetByMilestoneIdAsync(int milestoneId)
        {
            try
            {
                var bugs = await _httpClient.GetFromJsonAsync<List<BugResponse>>($"{ApiEndpoint}/milestone/{milestoneId}");
                return bugs ?? new List<BugResponse>();
            }
            catch (HttpRequestException)
            {
                return new List<BugResponse>();
            }
        }

        public async Task<List<BugResponse>> GetByProjectIdAsync(int projectId)
        {
            try
            {
                var bugs = await _httpClient.GetFromJsonAsync<List<BugResponse>>($"{ApiEndpoint}/project/{projectId}");
                return bugs ?? new List<BugResponse>();
            }
            catch (HttpRequestException)
            {
                return new List<BugResponse>();
            }
        }

        #endregion

        #region Bug Update Operations

        public async Task<bool> UpdateAsync(int id, UpdateBugRequest request)
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

        #endregion

        #region Bug Delete Operations

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
    }
}