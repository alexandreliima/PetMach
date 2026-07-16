using PetMach.Domain.SharedKernel;

namespace PetMach.Domain.Health;

public static class HealthErrors
{
    public static readonly DomainError ProofInvalid = new("health.proof_invalid", "O comprovante deve ser PDF, JPEG ou PNG e ter no máximo 5 MB.");
    public static readonly DomainError ProofNotFound = new("health.proof_not_found", "Comprovante não encontrado.");
}
