# ADR-003 — PostgreSQL como fonte de verdade

- Status: Aceita
- Data: 2026-07-13

## Contexto

Identidade, matches, mensagens, disponibilidade, reservas e auditoria exigem consistência relacional, índices e transações. Testes devem reproduzir o banco real.

## Decisão

Usar PostgreSQL com EF Core/Npgsql, migrations versionadas e Testcontainers. Não criar repositório genérico nem Unit of Work sobre `DbContext`. Constraints protegem e-mail, likes/matches e conflitos de reserva.

## Consequências

- Modelo e testes refletem semântica real do ambiente produtivo.
- Desenvolvimento/CI precisam de runtime de containers.
- Otimizações específicas devem continuar encapsuladas na Infrastructure.
