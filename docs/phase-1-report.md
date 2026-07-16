# Relatório — Fase 1

Data: 2026-07-13

## Resultado

A fundação do PetMach foi implementada como monólito modular separado em backend e frontend. O SDK .NET 10.0.301, workload MAUI Android e Android API/Build Tools 36 foram preparados localmente sem instalação administrativa global.

## Implementado

- Solução `PetMach.slnx` com 15 projetos e pastas lógicas.
- `global.json`, Central Package Management, build props, editorconfig e tool manifest.
- Domain/Application/Contracts/Infrastructure com dependências direcionadas.
- Result Pattern e catálogo dos 16 módulos.
- ASP.NET Core Identity, EF Core 10, Npgsql e migration `InitialIdentity`.
- API com `/api/v1`, OpenAPI, Problem Details, authorization fallback, rate limit, cache, SignalR e health checks.
- OpenTelemetry, resiliência HTTP e service discovery em Service Defaults.
- AppHost Aspire com PostgreSQL, API e Admin.
- Painel Blazor Interactive Server com política administrativa preparada.
- Aplicativo MAUI com XAML, MVVM, DI, Android e inclusão condicional de iOS no macOS.
- Dockerfiles não-root, Docker Compose e configuração segura por variável.
- Tarefas VS Code e scripts PowerShell.
- Testes unitários, arquiteturais, API integration e mobile com cobertura.

## Validação executada

- `dotnet format PetMach.slnx --verify-no-changes --no-restore`: aprovado.
- `dotnet build PetMach.slnx --no-restore`: aprovado, 0 warnings e 0 erros.
- `dotnet test PetMach.slnx --no-build --collect:'XPlat Code Coverage'`: 8 aprovados, 0 falhas, 0 ignorados.
- Build Android `net10.0-android`: aprovado como parte da solução.
- Migration inicial: gerada pelo `dotnet ef` e compilada.
- Vulnerabilidade transitiva detectada em `Microsoft.OpenApi` 2.0.0: removida por pin central em 2.10.0; restore seguinte sem o alerta NU1903.
- Smoke test do processo real: `/health/live` 200, `/api/v1/system` 200 e `/openapi/v1.json` 200.

## Limitações externas mantidas como gates

- Docker não está instalado: Docker Compose, PostgreSQL em container, Aspire dashboard e Testcontainers não foram executados.
- Não há Mac: build, assinatura e execução iOS não foram validados.
- CI remota foi adiada por decisão do responsável; gates locais são reproduzíveis.
- O sandbox desta sessão bloqueou o `llc.exe` baixado no SDK local. A validação Android utilizou o binutils assinado já instalado globalmente; isso não muda o projeto.
- O Git foi inicializado pelo usuário técnico do sandbox. Caso o Git do VS Code aplique a proteção de ownership, o README contém o comando `safe.directory` restrito à pasta PetMach.

## Próximo gate

A Fase 2 deve começar por Identity vertical: cadastro, confirmação de e-mail, login, access token, refresh rotativo/reuse detection, logout, consentimentos e testes de autorização. Antes dela, devem ser respondidas as perguntas de identidade em `docs/open-questions.md`.
