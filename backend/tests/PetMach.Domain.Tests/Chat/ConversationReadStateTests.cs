using FluentAssertions;
using PetMach.Domain.Chat;

namespace PetMach.Domain.Tests.Chat;

public sealed class ConversationReadStateTests
{
    private static readonly DateTimeOffset Now = new(2026, 7, 15, 9, 0, 0, TimeSpan.Zero);

    [Fact]
    public void ReadStateShouldOnlyAdvance()
    {
        Guid newestMessage = Guid.NewGuid();
        ConversationReadState state = new(Guid.NewGuid(), Guid.NewGuid(), newestMessage, Now, Now);

        state.Advance(Guid.NewGuid(), Now.AddMinutes(-1), Now.AddMinutes(1));

        state.LastReadMessageId.Should().Be(newestMessage);
        state.LastReadMessageAtUtc.Should().Be(Now);
    }

    [Fact]
    public void ReadStateShouldAcceptANewerMessage()
    {
        ConversationReadState state = new(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Now, Now);
        Guid newerMessage = Guid.NewGuid();

        state.Advance(newerMessage, Now.AddMinutes(1), Now.AddMinutes(2));

        state.LastReadMessageId.Should().Be(newerMessage);
        state.UpdatedAtUtc.Should().Be(Now.AddMinutes(2));
    }
}
