using PetMach.Domain.SharedKernel;

namespace PetMach.Domain.Discovery;

public static class DiscoveryErrors
{
    public static readonly DomainError Invalid = new("discovery.invalid", "A interação informada é inválida.");
    public static readonly DomainError DogNotFound = new("discovery.dog_not_found", "O cão não está disponível para esta interação.");
    public static readonly DomainError MatchNotFound = new("matches.not_found", "Match não encontrado.");
}
