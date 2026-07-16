# Plano de execução

## Fase 0 — Análise (concluída)

- Diagnóstico local e restrições de ferramenta.
- Escopo, premissas e perguntas.
- Arquitetura, módulos, modelo inicial, riscos e ADRs.
- `AGENTS.md`, workspace VS Code e separação backend/frontend.
- Gate: aprovação do responsável pelo produto.

## Fase 1 — Fundação (implementada em 2026-07-13)

- Instalar/validar .NET 10, workloads, Docker e Git.
- Criar `PetMach.slnx`, `global.json`, propriedades/pacotes centrais e projetos.
- Configurar API, Problem Details, OpenAPI, health checks, rate limiting e autenticação preparada.
- Configurar EF Core/PostgreSQL, primeira migration técnica e Aspire.
- Criar Admin e shell inicial MAUI compiláveis.
- Configurar observabilidade, Dockerfiles e compose de forma declarativa; disponibilizar gates locais de format, build, testes e cobertura. CI remota fica adiada até a escolha do provedor.
- Testes: smoke/arquitetura/health sem dependência externa. O boot com PostgreSQL/Testcontainers será habilitado assim que houver runtime de containers.

## Fase 2 — Identidade (implementada em 2026-07-14)

- Cadastro, confirmação, login, recuperação, refresh rotativo, logout e políticas.
- Consentimentos, papéis, suspensão e exclusão/anonimização inicial.
- Cliente mobile de autenticação e armazenamento seguro.
- Testes unitários, integração, autorização e reuse de refresh token.

## Fase 3 — Tutores, cães e saúde (em andamento desde 2026-07-14)

- Perfil do tutor, privacidade, cães, fotos, catálogo de raças e preferências.
- Vacinas, vermifugação, comprovantes protegidos e indicador público derivado.
- Fluxos MAUI de perfil, cadastro, galeria e vacinação.
- Testes de ownership, validação, uploads e privacidade.

## Fase 4 — Descoberta, match e bloqueio

- Consulta de candidatos, filtros e distância aproximada.
- Like/pass, reciprocidade, match único, unmatch, bloqueio e notificação.
- Telas MAUI de descoberta, filtros e matches.
- Testes de auto-like, duplicidade, suspensão, bloqueio e vazamento geográfico.

## Fase 5 — Chat e encontros

Estado: escopo funcional implementado em 2026-07-15; gate PostgreSQL/Testcontainers pendente.

- Conversas, SignalR, texto, histórico paginado, leitura e autorização por participante.
- Proposta e transições de encontros.
- Telas MAUI de conversas, chat e proposta.
- Testes de isolamento, bloqueio, paginação, reconexão e transições.

## Fase 6 — Parceiros, espaços e reservas

Estado: concluída em 2026-07-16, incluindo migrations e concorrência validadas em PostgreSQL real. A automação por Testcontainers continua opcional enquanto o pipe do Docker Desktop não estiver acessível ao runner.

- Parceiros, representantes, espaços, horários, disponibilidade e consultas.
- Reserva, confirmação/cancelamento, `IPaymentGateway` fake/presencial e histórico.
- Admin/parceiro e telas mobile correspondentes.
- Testes concorrentes de conflito e idempotência com PostgreSQL real.

## Fase 7 — Adoção, moderação e administração

Estado: iniciada em 2026-07-16.

- Área independente de adoção e histórico de estados/candidaturas definidas.
- Denúncias, evidências, fila de revisão, ações, suspensão e auditoria.
- Dashboard e gestões administrativas por política.
- Testes de permissão, retenção e ocultação.

## Fase 8 — Consolidação mobile

- Completar as 30 telas, navegação Shell, estados loading/empty/error/success e acessibilidade.
- Permissões just-in-time, geolocalização, mídia, caching e modo claro/escuro.
- Testes de ViewModels, navegação e clientes; builds Android/iOS em ambientes compatíveis.

## Fase 9 — Qualidade e release candidate

- Revisão de segurança/LGPD, ameaça, performance e concorrência.
- Testes completos, cobertura, migrations, backup/restore e observabilidade.
- Documentação operacional, API, banco, diagramas, Docker e Aspire.
- Build reproduzível e relatório final de riscos/pendências.

## Ritmo de cada incremento

1. Definir fatia vertical e critérios de aceite.
2. Atualizar contrato/teste antes ou junto da implementação.
3. Implementar domínio, caso de uso, persistência e endpoint/UI necessários.
4. Executar format, build e testes proporcionais; depois suíte completa no gate.
5. Atualizar documentação, ADR e backlog.
6. Demonstrar resultado e registrar riscos remanescentes.
