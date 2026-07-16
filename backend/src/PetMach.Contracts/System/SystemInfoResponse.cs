namespace PetMach.Contracts.System;

public sealed record SystemInfoResponse(
    string Service,
    string Version,
    string Environment,
    DateTimeOffset UtcNow);
