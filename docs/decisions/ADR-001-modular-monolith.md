# ADR-001 — Monólito modular

- Status: Aceita
- Data: 2026-07-13

## Contexto

O MVP reúne identidade, rede social, chat, localização, parceiros, reservas, adoção e moderação, mas ainda não possui carga, equipe ou limites operacionais comprovados que justifiquem serviços distribuídos.

## Decisão

Construir um monólito modular em .NET, implantado inicialmente como API principal, com painel Admin e mobile separados. Organizar Domain/Application/Infrastructure por módulo e impor dependências com testes arquiteturais e contratos explícitos. PostgreSQL é compartilhado, com ownership lógico dos dados por módulo.

## Consequências

- Menor custo operacional, transações locais e depuração simples.
- Exige disciplina para impedir acoplamento entre módulos.
- Chat/Notifications e Reservations são candidatos a extração futura somente mediante métricas e novo ADR.
