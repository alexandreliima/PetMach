using FluentValidation;
using PetMach.Contracts.Dogs;

namespace PetMach.Application.Dogs;

public sealed class DogValidator : AbstractValidator<UpsertDogRequest>
{
    public DogValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Breed).NotEmpty().MaximumLength(120);
        RuleFor(x => x.WeightKg).GreaterThan(0).LessThanOrEqualTo(150).When(x => x.WeightKg.HasValue);
        RuleFor(x => x.Temperament).NotEmpty().MaximumLength(500);
        RuleFor(x => x.SociabilityWithDogs).InclusiveBetween(1, 5);
        RuleFor(x => x.SociabilityWithPeople).InclusiveBetween(1, 5);
        RuleFor(x => x.SociabilityWithChildren).InclusiveBetween(1, 5);
        RuleFor(x => x.Restrictions).MaximumLength(1000);
        RuleFor(x => x.SpecialNeeds).MaximumLength(1000);
        RuleFor(x => x.Biography).MaximumLength(2000);
    }
}
