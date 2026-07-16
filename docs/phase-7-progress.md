# Progresso — Fase 7

Data de início: 2026-07-16

## Incremento 1 — publicações de adoção

- Área e contratos independentes de descoberta, likes e matches.
- Somente tutor publica um cão próprio e ativo, mediante aceite explícito do termo versionado.
- Uma publicação por cão, protegida por constraint única no PostgreSQL.
- Catálogo autenticado limitado a 100 itens e filtrado por bloqueios nos dois sentidos.
- Região respeita `ShowCity` e nunca expõe coordenadas ou localização exata.
- Proprietário pode suspender a própria publicação; itens suspensos continuam visíveis apenas para ele.

## Incremento 2 — candidaturas formais

- Candidato informa motivação, experiência e contexto do lar, com aceite explícito do termo versionado.
- Não é permitido candidatar-se à própria publicação nem atravessar bloqueios.
- Uma candidatura por pessoa/publicação e no máximo uma candidatura aprovada por publicação, garantidas no PostgreSQL.
- Fluxo explícito: `Submitted`, `UnderReview`, `Approved`, `Rejected` ou `Withdrawn`.
- Somente o responsável revisa, aprova ou rejeita; somente o candidato retira.
- Aprovação move a publicação para `InProgress`, sem finalizar automaticamente a adoção.
- Histórico registra ator, estado anterior, novo estado e instante UTC.
- A constraint de aprovação única foi validada com duas transações concorrentes no PostgreSQL real, retornando `23505` para a disputa perdedora.

## Próximos incrementos

1. Denúncias, evidências protegidas e fila de moderação.
2. Ações administrativas auditadas.
3. Experiência mobile de adoção.

A Fase 7 está em andamento.
