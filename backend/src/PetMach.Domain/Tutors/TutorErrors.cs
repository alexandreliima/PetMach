using PetMach.Domain.SharedKernel;

namespace PetMach.Domain.Tutors;

public static class TutorErrors
{
    public static readonly DomainError InvalidProfile = new("tutor.invalid_profile", "Os dados do perfil são inválidos.");
    public static readonly DomainError NotFound = new("tutor.not_found", "O perfil do tutor não foi encontrado.");
}
