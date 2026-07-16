using FluentAssertions;
using PetMach.Domain.Health;

namespace PetMach.Domain.Tests.Health;

public sealed class DogHealthTests
{
    private static readonly DateTimeOffset Now = new(2026, 7, 14, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void VaccinationShouldAcceptAChronologicalNextDose()
    {
        DogVaccination vaccination = new(Guid.NewGuid(), "V10", new DateOnly(2026, 7, 1), new DateOnly(2027, 7, 1), Now);

        vaccination.VaccineName.Should().Be("V10");
        vaccination.NextDoseOn.Should().Be(new DateOnly(2027, 7, 1));
    }

    [Fact]
    public void DewormingShouldRejectNextDoseBeforeApplication()
    {
        Action create = () => _ = new DewormingRecord(Guid.NewGuid(), "Produto", new DateOnly(2026, 7, 10), new DateOnly(2026, 7, 1), Now);

        create.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void VaccinationShouldStoreOnlyAnInternalProofKey()
    {
        DogVaccination vaccination = new(Guid.NewGuid(), "V10", new DateOnly(2026, 7, 1), null, Now);

        vaccination.AttachProof("health/dog/generated.pdf");

        vaccination.ProtectedProofKey.Should().Be("health/dog/generated.pdf");
    }
}
