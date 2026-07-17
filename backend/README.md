# Backend

Monólito modular PetMach em .NET 10. Contém API, Domain, Application,
Contracts, Infrastructure, Service Defaults, Admin, Aspire AppHost e quatro
projetos de testes.

O banco é PostgreSQL via EF Core/Npgsql. As 18 migrations atuais ficam em
`src/PetMach.Infrastructure/Persistence/Migrations`.

## Processos executáveis

- `PetMach.Api`: API HTTP `/api/v1`, SignalR e health checks;
- `PetMach.Admin`: painel Blazor server-side que consome a API;
- `PetMach.AppHost`: PostgreSQL, API e Admin orquestrados por Aspire.

No Docker Compose, um target dedicado do Dockerfile da API executa as
migrations antes da inicialização da API. API e Admin usam o `APP_UID` não
privilegiado das imagens oficiais .NET.

## Testes PostgreSQL

`tests/PetMach.Api.IntegrationTests` contém cinco testes
`Category=PostgreSQL`. A fixture inicia `postgres:18.0-alpine` via
Testcontainers, aplica todas as migrations, limpa os dados entre testes e falha
explicitamente quando Docker não está disponível.

```powershell
dotnet test backend/tests/PetMach.Api.IntegrationTests/PetMach.Api.IntegrationTests.csproj --filter 'Category=PostgreSQL'
```

Consulte [estado técnico](../docs/current-state.md),
[operação](../docs/operations.md) e [testes](../docs/testing.md).
