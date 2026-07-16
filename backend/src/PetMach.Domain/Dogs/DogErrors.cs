using PetMach.Domain.SharedKernel;

namespace PetMach.Domain.Dogs;

public static class DogErrors
{
    public static readonly DomainError Invalid = new("dog.invalid", "Os dados do cão são inválidos.");
    public static readonly DomainError NotFound = new("dog.not_found", "O cão não foi encontrado.");
    public static readonly DomainError PhotoInvalid = new("dog.photo_invalid", "A foto deve ser JPEG, PNG ou WebP e ter no máximo 5 MB.");
}
