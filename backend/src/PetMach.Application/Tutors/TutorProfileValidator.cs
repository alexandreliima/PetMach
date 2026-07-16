using FluentValidation;
using PetMach.Contracts.Tutors;

namespace PetMach.Application.Tutors;

public sealed class TutorProfileValidator : AbstractValidator<UpsertTutorProfileRequest>
{
    public TutorProfileValidator()
    {
        RuleFor(request => request.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(request => request.LastName).NotEmpty().MaximumLength(100);
        RuleFor(request => request.Phone).MaximumLength(30);
        RuleFor(request => request.City).NotEmpty().MaximumLength(120);
        RuleFor(request => request.State).NotEmpty().MinimumLength(2).MaximumLength(50);
        RuleFor(request => request.Biography).MaximumLength(1000);
    }
}
