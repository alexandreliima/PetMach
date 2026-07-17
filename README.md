# PetMach

Plataforma de socialização, experiências e serviços para cães e seus tutores. O PetMach começa como um **monólito modular**, com backend e frontend separados e limites arquiteturais testados.

## Estado atual

O repositório contém as fatias funcionais de identidade, tutores, pets, saúde,
descoberta, matches, chat, encontros, parceiros, reservas, adoção e moderação.
O Mobile possui navegação e ciclo de sessão estabilizados, e o Admin oferece o
fluxo administrativo de denúncias e ações de moderação.

PostgreSQL real está validado em dois caminhos:

- os testes de persistência iniciam `postgres:18.0-alpine` automaticamente via
  Testcontainers, aplicam as 18 migrations e exercitam constraints
  concorrentes;
- Docker Compose inicia PostgreSQL, migrator, API e Admin com health checks e
  dependências por prontidão.

Consulte [Estado técnico atual](docs/current-state.md),
[Operação e execução](docs/operations.md) e [Testes](docs/testing.md). Os
relatórios `docs/phase-*.md` são registros históricos e podem mencionar
limitações removidas posteriormente.

## Fundação disponível

- ASP.NET Core Web API com `/api/v1`, OpenAPI, Problem Details e controllers por feature.
- Rate limiting global, output cache, autenticação/autorizações preparadas e SignalR.
- ASP.NET Core Identity com IDs `Guid`, refresh rotativo e migrations
  PostgreSQL.
- EF Core/Npgsql sem repositório genérico ou Unit of Work redundante.
- Health checks de liveness/readiness, OpenTelemetry e service discovery.
- Blazor Web App Interactive Server para administração.
- .NET Aspire AppHost com API, Admin e PostgreSQL.
- .NET MAUI com XAML, MVVM, DI, sessão em `SecureStorage`, navegação por troca
  de raiz e Shell autenticada; Android local e iOS condicionado a macOS.
- Testes de domínio, aplicação, arquitetura, API, persistência PostgreSQL e
  núcleo mobile.
- Central Package Management, analyzers, nullable e warnings como erros em Domain/Application.
- Dockerfiles multi-stage executando com usuário não privilegiado, migrator
  dedicado, Docker Compose, scripts e tarefas para VS Code.

## Estrutura

```text
PetMach/
├── backend/
│   ├── src/
│   │   ├── PetMach.Api/
│   │   ├── PetMach.Application/
│   │   ├── PetMach.Contracts/
│   │   ├── PetMach.Domain/
│   │   ├── PetMach.Infrastructure/
│   │   ├── PetMach.ServiceDefaults/
│   │   ├── PetMach.Admin/
│   │   └── PetMach.AppHost/
│   └── tests/
├── frontend/
│   ├── src/
│   │   ├── PetMach.Mobile.Core/
│   │   └── PetMach.Mobile/
│   └── tests/
├── docs/
├── scripts/
├── PetMach.slnx
└── PetMach.code-workspace
```

## Pré-requisitos

- .NET SDK 10.0.301 ou patch compatível da feature band, conforme `global.json`.
- Workload `maui-android` e Android SDK Platform/Build Tools 36 para o app Android.
- PostgreSQL 18 para persistência local, ou Docker quando disponível.
- Mac com toolchain Apple para validar/assinar iOS.
- Visual Studio Code com as extensões recomendadas pelo workspace.

Nesta máquina, o SDK e workloads foram instalados localmente em `.dotnet/`, e o Android SDK em `.android-sdk/`. Ambos estão ignorados pelo Git.

## Abrir no Visual Studio Code

Abra `PetMach.code-workspace`. As tarefas **PetMach: restore**, **PetMach: build**, **PetMach: test**, **PetMach: API** e **PetMach: Admin** ficam disponíveis em `Terminal > Run Task`.

O Git foi inicializado pela sessão protegida do Codex. Se o VS Code exibir `unsafe repository`, execute uma vez no seu terminal de usuário:

```powershell
git config --global --add safe.directory 'C:/Users/alima/Desktop/Meus Doc/Projetos IA/PetMach'
```

O comando libera somente esta pasta; não use `safe.directory=*`.

## Executar

No PowerShell, na raiz:

```powershell
$dotnet = if (Test-Path .\.dotnet\dotnet.exe) { '.\.dotnet\dotnet.exe' } else { 'dotnet' }
& $dotnet restore PetMach.slnx
& $dotnet run --project backend/src/PetMach.Api/PetMach.Api.csproj
```

Endpoints iniciais:

- `GET /api/v1/system`
- `GET /health/live`
- `GET /health/ready`
- `/openapi/v1.json` em Development
- `/hubs/chat` autenticado

Endpoints de identidade:

- `POST /api/v1/auth/register`
- `POST /api/v1/auth/confirm-email`
- `POST /api/v1/auth/login`
- `POST /api/v1/auth/refresh`
- `POST /api/v1/auth/logout`
- `POST /api/v1/auth/forgot-password`
- `POST /api/v1/auth/reset-password`
- `GET /api/v1/auth/me`
- `DELETE /api/v1/auth/account`
- `PATCH /api/v1/administration/users/{id}/suspension`
- `GET /api/v1/tutors/me`
- `PUT /api/v1/tutors/me`

Em Development, confirmações e recuperações são capturadas em `backend/src/PetMach.Api/.dev-emails/`. A pasta é ignorada pelo Git e pode conter tokens; não compartilhe seu conteúdo.

Para o Admin:

```powershell
& $dotnet run --project backend/src/PetMach.Admin/PetMach.Admin.csproj
```

Para Android:

```powershell
& $dotnet build frontend/src/PetMach.Mobile/PetMach.Mobile.csproj -f net10.0-android
```

Com um emulador ou aparelho Android conectado:

```powershell
& $dotnet build frontend/src/PetMach.Mobile/PetMach.Mobile.csproj -f net10.0-android -t:Run
```

No emulador Android, o cliente usa `http://10.0.2.2:5049`, que aponta para a API executada no computador. O tráfego HTTP local é permitido somente em build Debug.

## Banco e migrations

A conexão é lida de `ConnectionStrings__petmach`. A configuração Development sem senha serve apenas como base; informe credenciais por User Secrets ou variável de ambiente.

```powershell
& $dotnet user-secrets init --project backend/src/PetMach.Api/PetMach.Api.csproj
& $dotnet user-secrets set 'ConnectionStrings:petmach' 'Host=localhost;Port=5432;Database=petmach;Username=petmach;Password=SENHA_LOCAL' --project backend/src/PetMach.Api/PetMach.Api.csproj
& $dotnet tool restore
& $dotnet ef database update --project backend/src/PetMach.Infrastructure/PetMach.Infrastructure.csproj --startup-project backend/src/PetMach.Api/PetMach.Api.csproj
```

Nova migration:

```powershell
& $dotnet ef migrations add NomeDaMigration --project backend/src/PetMach.Infrastructure/PetMach.Infrastructure.csproj --startup-project backend/src/PetMach.Api/PetMach.Api.csproj --output-dir Persistence/Migrations
```

## Docker e Aspire

Quando Docker estiver disponível:

```powershell
Copy-Item .env.example .env
# Edite .env e substitua os dois placeholders.
docker compose config
docker compose build
docker compose up -d --wait --wait-timeout 180
docker compose ps -a
```

O Compose publica a API em `http://localhost:5080` e o Admin em
`http://localhost:5081`. Internamente, o Admin acessa a API por
`http://api:8080`; `localhost` nunca é usado entre containers. PostgreSQL,
API e Admin possuem health checks, e cada serviço aguarda a prontidão de sua
dependência.

No Compose, um serviço `migrator` executa as 18 migrations reais e termina com
exit code `0` antes da API ser iniciada. Há uma única instância do migrator na
composição, evitando corrida entre instâncias da API. Os containers finais de
API e Admin executam com o `APP_UID` não privilegiado das imagens oficiais
.NET. Em execução local comum e no Aspire, o comando explícito
`dotnet ef database update` continua disponível.

Ou execute o AppHost:

```powershell
& $dotnet run --project backend/src/PetMach.AppHost/PetMach.AppHost.csproj
```

No Aspire, a API recebe a conexão PostgreSQL por referência do AppHost e o
Admin resolve a API via service discovery. Fora de Development, sem service
discovery, `PetMachApi__BaseUrl` é obrigatório.

Nenhuma senha real deve ser gravada no repositório. `.env.example` contém somente um placeholder.

O runbook completo, incluindo verificações e encerramento seguro, está em
[docs/operations.md](docs/operations.md).

## Qualidade e testes

```powershell
.\scripts\quality.ps1
```

O script executa restore, format check, build, testes e cobertura. Individualmente:

```powershell
& $dotnet format PetMach.slnx --verify-no-changes --no-restore
& $dotnet build PetMach.slnx --no-restore
& $dotnet test PetMach.slnx --no-build --collect:'XPlat Code Coverage'
```

Os testes `Category=PostgreSQL` iniciam PostgreSQL 18 automaticamente com
Testcontainers e falham de forma explícita quando Docker não está disponível:

```powershell
& $dotnet test backend/tests/PetMach.Api.IntegrationTests/PetMach.Api.IntegrationTests.csproj --filter 'Category=PostgreSQL'
```

O resultado esperado atual é 5 testes aprovados, sem falhas ou ignorados. A
fixture aplica as 18 migrations, limpa os dados entre casos e valida constraints
concorrentes de reservas e adoção. Detalhes em
[docs/testing.md](docs/testing.md).

## Variáveis principais

| Variável | Finalidade |
|---|---|
| `ConnectionStrings__petmach` | Conexão PostgreSQL da API |
| `PETMACH_POSTGRES_PASSWORD` | Senha exigida pelo Docker Compose |
| `OTEL_EXPORTER_OTLP_ENDPOINT` | Coletor OpenTelemetry opcional |
| `ASPNETCORE_ENVIRONMENT` | Ambiente ASP.NET Core |
| `Identity__SigningKey` | Chave JWT com ao menos 32 bytes; obrigatória fora de Development |
| `PetMachApi__BaseUrl` | URL da API consumida pelo Admin; no Compose use `http://api:8080/` |

## Convenções

- IDs `Guid`, datas relevantes em UTC com `DateTimeOffset`.
- Domain não depende de Application/Infrastructure/API.
- Entidades nunca são contratos HTTP.
- Autorização é negada por padrão; exceções públicas são explícitas.
- Coordenadas exatas, tokens, senhas e saúde sensível nunca entram em logs.
- Decisões arquiteturais são registradas em `docs/decisions/`.

## Roadmap

As fases 1–7 possuem implementação funcional no repositório. Os incrementos
técnicos posteriores estabilizaram Testcontainers, sessão/navegação Mobile e a
operação Docker de Admin/API/PostgreSQL.

Os próximos gates concentram-se em consolidação visual e acessibilidade Mobile,
políticas definitivas de retenção/LGPD, validação iOS em macOS, segurança,
performance, backup/restore e preparação do release candidate.

Consulte `docs/execution-plan.md`, `docs/architecture.md` e `AGENTS.md`.
