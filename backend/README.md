# Backend

Monólito modular PetMach em .NET 10. Contém API, Domain, Application, Contracts, Infrastructure, Service Defaults, Admin, Aspire AppHost e quatro projetos de testes.

O banco é PostgreSQL via EF Core/Npgsql. A migration inicial de Identity está em `src/PetMach.Infrastructure/Persistence/Migrations`.
