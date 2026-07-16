# Relatório — Fase 2

Data: 2026-07-14

## Resultado

A fatia vertical de identidade foi implementada no backend e no núcleo mobile. Ela cobre cadastro, confirmação, login, recuperação, sessões, consentimento, ciclo básico de conta, papéis e políticas.

## Implementado

- Contratos HTTP versionados para registro, confirmação, login, refresh, logout, recuperação, conta e suspensão.
- Validadores FluentValidation para entradas críticas.
- ASP.NET Core Identity com idade, estado da conta, confirmação obrigatória e senha mínima de 12 caracteres.
- JWT de 15 minutos e refresh de 30 dias configuráveis.
- Refresh token aleatório armazenado somente como hash, uso único, rotação, família, revogação e detecção de reutilização/concorrência.
- Consentimentos versionados para termos e privacidade.
- Recuperação de senha sem enumeração de contas e capturador local de e-mail em Development.
- Anonimização imediata, revogação de sessões, suspensão/reativação e auditoria técnica.
- Políticas `TutorAccess`, `PartnerAccess` e `AdministrationAccess`.
- Duas migrations: `Phase2Identity` e `Phase2IdentityAudit`.
- Cliente mobile de login/refresh/logout e armazenamento de tokens via MAUI `SecureStorage`.
- Testes de domínio, validação, contratos/autorização da API e sessão mobile.

## Validação desta entrega

- `dotnet format PetMach.slnx --verify-no-changes --no-restore`: aprovado.
- Build dos 15 projetos, incluindo Android: aprovado, 0 warnings e 0 erros, usando o binutils global assinado exigido por este sandbox.
- Testes com cobertura: 22 aprovados, 0 falhas, 0 ignorados, somando todas as suítes.
- `dotnet ef migrations has-pending-model-changes`: aprovado, sem mudanças de modelo pendentes.

## Limitações externas

- Docker/PostgreSQL local continuam indisponíveis. A migration foi gerada, mas os testes de persistência/Testcontainers e a aplicação em banco real permanecem como gate obrigatório assim que houver runtime de containers.
- A consulta online de vulnerabilidades do NuGet retornou `NU1900` por indisponibilidade de acesso a `api.nuget.org`. O restore do gate limpo usou `NuGetAudit=false` apenas na linha de comando; a auditoria não foi desabilitada no repositório.
- Provedor de e-mail, textos jurídicos, retenção e acúmulo de papéis continuam como decisões abertas; defaults provisórios estão documentados.

## Próximo gate

Aplicar as migrations em PostgreSQL e executar Testcontainers quando Docker estiver disponível. Depois da aprovação funcional da identidade, iniciar a Fase 3: tutores, cães, fotos e saúde.
