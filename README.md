# PetMach

Plataforma de socialização, experiências e serviços para cães e seus tutores. O PetMach começa como um **monólito modular**, com backend e frontend separados e limites arquiteturais testados.

## Estado

Fase 3 em andamento. A identidade está implementada e o aplicativo já possui fluxo visual de boas-vindas, cadastro, login e home inicial. A primeira fatia de perfil do tutor também está disponível na API.

Docker/Aspire com containers e o build iOS ainda não foram executados porque não há Docker nem Mac disponíveis nesta máquina. Essas limitações não removem os projetos ou configurações correspondentes.

## Fundação disponível

- ASP.NET Core Web API com `/api/v1`, OpenAPI, Problem Details e controllers por feature.
- Rate limiting global, output cache, autenticação/autorizações preparadas e SignalR.
- ASP.NET Core Identity com IDs `Guid` e migration inicial PostgreSQL.
- EF Core/Npgsql sem repositório genérico ou Unit of Work redundante.
- Health checks de liveness/readiness, OpenTelemetry e service discovery.
- Blazor Web App Interactive Server para administração.
- .NET Aspire AppHost com API, Admin e PostgreSQL.
- .NET MAUI com XAML, MVVM, DI e shell inicial; Android local e iOS condicionado a macOS.
- Testes de domínio, aplicação, arquitetura, API e ViewModel mobile.
- Central Package Management, analyzers, nullable e warnings como erros em Domain/Application.
- Dockerfiles, Docker Compose, scripts e tarefas para VS Code.

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
$env:PETMACH_POSTGRES_PASSWORD = 'uma-senha-local-forte'
$env:Identity__SigningKey = 'uma-chave-local-com-pelo-menos-32-caracteres'
docker compose up --build
```

O Compose publica a API em `http://localhost:5080` e o Admin em
`http://localhost:5081`. Internamente, o Admin acessa a API por
`http://api:8080`; `localhost` nunca é usado entre containers. PostgreSQL,
API e Admin possuem health checks, e cada serviço aguarda a prontidão de sua
dependência.

No Compose, um serviço `migrator` executa as migrations reais e termina antes
da API ser iniciada. Há uma única instância do migrator na composição, evitando
corrida entre instâncias da API. Em execução local comum e no Aspire, o comando
explícito `dotnet ef database update` continua disponível.

Ou execute o AppHost:

```powershell
& $dotnet run --project backend/src/PetMach.AppHost/PetMach.AppHost.csproj
```

No Aspire, a API recebe a conexão PostgreSQL por referência do AppHost e o
Admin resolve a API via service discovery. Fora de Development, sem service
discovery, `PetMachApi__BaseUrl` é obrigatório.

Nenhuma senha real deve ser gravada no repositório. `.env.example` contém somente um placeholder.

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

Testes de persistência com Testcontainers estão referenciados, mas sua execução será introduzida junto dos casos de uso que persistem dados e requer Docker disponível.

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

1. Fases 1–2: fundação e identidade concluídas.
2. Fase 3: tutores, cães, fotos e saúde.
3. Fase 4: descoberta, likes, matches e bloqueios.
4. Fase 5: chat e encontros com escopo funcional implementado; validação PostgreSQL pendente.
5. Fase 6: parceiros, espaços e reservas (em andamento).
6. Fase 7: adoção, moderação e administração.
7. Fases 8–9: consolidação mobile, segurança, performance e release candidate.

Consulte `docs/phase-1-report.md`, `docs/architecture.md` e `AGENTS.md`.
