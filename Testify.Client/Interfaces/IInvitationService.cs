using Testify.Shared.DTOs.Invitations;

namespace Testify.Client.Interfaces
{
    public interface IInvitationService
    {
        Task<InvitationResponse> SendInvitationAsync(SendInvitationRequest request);
        Task<List<PendingInvitationResponse>> GetPendingInvitationsAsync(int projectId);
        Task<bool> RevokeInvitationAsync(long invitationId);
    }
}
