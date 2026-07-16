using FluentAssertions;
using PetMach.Domain.SharedKernel;

namespace PetMach.Domain.Tests.SharedKernel;

public sealed class ResultTests
{
    [Fact]
    public void SuccessShouldExposeValue()
    {
        Result<string> result = Result.Success("petmach");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("petmach");
        result.Error.Should().Be(DomainError.None);
    }

    [Fact]
    public void FailureShouldNotExposeValue()
    {
        Result<string> result = Result.Failure<string>(new DomainError("test.failure", "Falha esperada."));

        result.IsFailure.Should().BeTrue();
        Action readValue = () => _ = result.Value;
        readValue.Should().Throw<InvalidOperationException>();
    }
}
