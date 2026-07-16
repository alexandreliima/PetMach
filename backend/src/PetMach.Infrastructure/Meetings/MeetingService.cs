using Microsoft.EntityFrameworkCore;
using PetMach.Application.Meetings;
using PetMach.Contracts.Meetings;
using PetMach.Domain.Meetings;
using PetMach.Domain.Notifications;
using PetMach.Domain.SharedKernel;
using PetMach.Infrastructure.Persistence;

namespace PetMach.Infrastructure.Meetings;

internal sealed class MeetingService(PetMachDbContext dbContext, TimeProvider timeProvider) : IMeetingService
{
    private static readonly DomainError Invalid = new("meetings.invalid", "A proposta de encontro é inválida.");
    private static readonly DomainError NotFound = new("meetings.not_found", "Encontro não encontrado.");

    public async Task<IReadOnlyCollection<MeetingResponse>> ListAsync(Guid userId, CancellationToken cancellationToken) =>
        await ParticipantMeetings(userId).AsNoTracking().OrderByDescending(x => x.CreatedAtUtc)
            .Select(x => new MeetingResponse(x.Id, x.MatchId, x.ProposedByUserId, x.ScheduledAtUtc, x.PlaceName, x.Notes, (MeetingStatusContract)x.Status,
                x.Status == MeetingStatus.Proposed && x.ProposedByUserId != userId,
                x.Status == MeetingStatus.Proposed || x.Status == MeetingStatus.Accepted,
                x.CreatedAtUtc, x.UpdatedAtUtc))
            .ToArrayAsync(cancellationToken);

    public async Task<Result<MeetingResponse>> CreateAsync(Guid userId, CreateMeetingRequest request, CancellationToken cancellationToken)
    {
        DateTimeOffset now = timeProvider.GetUtcNow();
        if (request.ScheduledAtUtc <= now || string.IsNullOrWhiteSpace(request.PlaceName) || request.PlaceName.Trim().Length > 160 || request.Notes?.Trim().Length > 1000)
            return Result.Failure<MeetingResponse>(Invalid);
        if (!await IsActiveMatchParticipantAsync(userId, request.MatchId, cancellationToken)) return Result.Failure<MeetingResponse>(NotFound);
        DogMeeting meeting = new(request.MatchId, userId, request.ScheduledAtUtc, request.PlaceName, request.Notes, now);
        dbContext.DogMeetings.Add(meeting);
        MatchParticipants? participants = await GetParticipantsAsync(request.MatchId, cancellationToken);
        Guid recipient = participants!.Other(userId);
        dbContext.UserNotifications.Add(UserNotification.ForMeeting(recipient, meeting.Id, "meeting.proposed", "Nova proposta de encontro", "Um tutor propôs um encontro para os cães.", now));
        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success(ToResponse(meeting, userId));
    }

    public Task<Result<MeetingResponse>> AcceptAsync(Guid userId, Guid meetingId, CancellationToken cancellationToken) =>
        TransitionAsync(userId, meetingId, "meeting.accepted", "Encontro aceito", "A proposta de encontro foi aceita.", (meeting, now) => meeting.Accept(userId, now), cancellationToken);

    public Task<Result<MeetingResponse>> DeclineAsync(Guid userId, Guid meetingId, CancellationToken cancellationToken) =>
        TransitionAsync(userId, meetingId, "meeting.declined", "Encontro recusado", "A proposta de encontro foi recusada.", (meeting, now) => meeting.Decline(userId, now), cancellationToken);

    public Task<Result<MeetingResponse>> CancelAsync(Guid userId, Guid meetingId, CancellationToken cancellationToken) =>
        TransitionAsync(userId, meetingId, "meeting.cancelled", "Encontro cancelado", "A proposta de encontro foi cancelada.", (meeting, now) => meeting.Cancel(now), cancellationToken);

    private async Task<Result<MeetingResponse>> TransitionAsync(Guid userId, Guid meetingId, string notificationType, string title, string message, Func<DogMeeting, DateTimeOffset, bool> transition, CancellationToken cancellationToken)
    {
        DogMeeting? meeting = await ParticipantMeetings(userId).SingleOrDefaultAsync(x => x.Id == meetingId, cancellationToken);
        if (meeting is null || !await IsActiveMatchParticipantAsync(userId, meeting.MatchId, cancellationToken)) return Result.Failure<MeetingResponse>(NotFound);
        DateTimeOffset now = timeProvider.GetUtcNow();
        if (!transition(meeting, now)) return Result.Failure<MeetingResponse>(Invalid);
        MatchParticipants? participants = await GetParticipantsAsync(meeting.MatchId, cancellationToken);
        dbContext.UserNotifications.Add(UserNotification.ForMeeting(participants!.Other(userId), meeting.Id, notificationType, title, message, now));
        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success(ToResponse(meeting, userId));
    }

    private IQueryable<DogMeeting> ParticipantMeetings(Guid userId) =>
        from meeting in dbContext.DogMeetings
        join match in dbContext.DogMatches on meeting.MatchId equals match.Id
        where dbContext.Dogs.Any(dog => (dog.Id == match.DogAId || dog.Id == match.DogBId) && dog.OwnerUserId == userId)
        select meeting;

    private Task<bool> IsActiveMatchParticipantAsync(Guid userId, Guid matchId, CancellationToken cancellationToken) =>
        (from match in dbContext.DogMatches.AsNoTracking()
         join dogA in dbContext.Dogs.AsNoTracking() on match.DogAId equals dogA.Id
         join dogB in dbContext.Dogs.AsNoTracking() on match.DogBId equals dogB.Id
         where match.Id == matchId && match.EndedAtUtc == null && (dogA.OwnerUserId == userId || dogB.OwnerUserId == userId) &&
             !dbContext.BlockedUsers.Any(block => (block.UserId == dogA.OwnerUserId && block.BlockedUserId == dogB.OwnerUserId) || (block.UserId == dogB.OwnerUserId && block.BlockedUserId == dogA.OwnerUserId))
         select match.Id).AnyAsync(cancellationToken);

    private Task<MatchParticipants?> GetParticipantsAsync(Guid matchId, CancellationToken cancellationToken) =>
        (from match in dbContext.DogMatches.AsNoTracking()
         join dogA in dbContext.Dogs.AsNoTracking() on match.DogAId equals dogA.Id
         join dogB in dbContext.Dogs.AsNoTracking() on match.DogBId equals dogB.Id
         where match.Id == matchId
         select new MatchParticipants(dogA.OwnerUserId, dogB.OwnerUserId)).SingleOrDefaultAsync(cancellationToken);

    private static MeetingResponse ToResponse(DogMeeting meeting, Guid userId) => new(
        meeting.Id, meeting.MatchId, meeting.ProposedByUserId, meeting.ScheduledAtUtc, meeting.PlaceName, meeting.Notes,
        (MeetingStatusContract)meeting.Status,
        meeting.Status == MeetingStatus.Proposed && meeting.ProposedByUserId != userId,
        meeting.Status is MeetingStatus.Proposed or MeetingStatus.Accepted,
        meeting.CreatedAtUtc, meeting.UpdatedAtUtc);

    private sealed record MatchParticipants(Guid UserAId, Guid UserBId)
    {
        public Guid Other(Guid userId) => UserAId == userId ? UserBId : UserAId;
    }
}
