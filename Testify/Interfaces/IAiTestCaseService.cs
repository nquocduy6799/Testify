using Testify.Shared.DTOs.AI;

namespace Testify.Interfaces
{
    public interface IAiTestCaseService
    {
        Task<AiGenerateTestCasesResponse> GenerateTestCasesAsync(AiGenerateTestCasesRequest request);
    }
}
