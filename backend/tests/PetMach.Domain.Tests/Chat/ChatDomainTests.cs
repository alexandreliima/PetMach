using FluentAssertions;
using PetMach.Domain.Chat;

namespace PetMach.Domain.Tests.Chat;

public sealed class ChatDomainTests
{
    private static readonly DateTimeOffset Now = new(2026, 7, 14, 20, 0, 0, TimeSpan.Zero);

    [Fact]
    public void MessageShouldTrimValidContent()
    {
        ChatMessage message = new(Guid.NewGuid(), Guid.NewGuid(), "  Olá!  ", Now);
        message.Content.Should().Be("Olá!");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void MessageShouldRejectBlankContent(string content)
    {
        Action create = () => _ = new ChatMessage(Guid.NewGuid(), Guid.NewGuid(), content, Now);
        create.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void MessageShouldRejectContentOverLimit()
    {
        Action create = () => _ = new ChatMessage(Guid.NewGuid(), Guid.NewGuid(), new string('a', 2001), Now);
        create.Should().Throw<ArgumentException>();
    }
}
