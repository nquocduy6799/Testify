using Testify.Shared.DTOs.AI;

namespace Testify.Client.Interfaces
{
    public interface IAiTestCaseService
    {
        Task<AiGenerateTestCasesResponse> GenerateAsync(AiGenerateTestCasesRequest request);
        Task<AiUsageResponse> GetUsageAsync();
    }
}
