using PetMach.Contracts.Meetings;
using PetMach.Domain.SharedKernel;

namespace PetMach.Application.Meetings;

public interface IMeetingService
{
    Task<IReadOnlyCollection<MeetingResponse>> ListAsync(Guid userId, CancellationToken cancellationToken);
    Task<Result<MeetingResponse>> CreateAsync(Guid userId, CreateMeetingRequest request, CancellationToken cancellationToken);
    Task<Result<MeetingResponse>> AcceptAsync(Guid userId, Guid meetingId, CancellationToken cancellationToken);
    Task<Result<MeetingResponse>> DeclineAsync(Guid userId, Guid meetingId, CancellationToken cancellationToken);
    Task<Result<MeetingResponse>> CancelAsync(Guid userId, Guid meetingId, CancellationToken cancellationToken);
}
