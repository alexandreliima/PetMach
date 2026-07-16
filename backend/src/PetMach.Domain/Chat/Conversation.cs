namespace PetMach.Domain.Chat;

public sealed class Conversation
{
    private Conversation() { }

    public Conversation(Guid matchId, DateTimeOffset now)
    {
        if (matchId == Guid.Empty) throw new ArgumentException("Match inválido.", nameof(matchId));
        Id = Guid.NewGuid();
        MatchId = matchId;
        CreatedAtUtc = now;
    }

    public Guid Id { get; private set; }
    public Guid MatchId { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
}
