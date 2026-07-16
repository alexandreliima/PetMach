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

## Incremento 3 — denúncias e fila de moderação

- Denúncias tipadas para usuário, cão, publicação de adoção ou mensagem de chat.
- Motivos usam allowlist e o denunciante não pode denunciar conteúdo próprio.
- Duplicidade ativa por denunciante/alvo é impedida por constraint parcial única.
- Evidências aceitam somente JPEG, PNG ou PDF real, até 5 MB e cinco arquivos por denúncia.
- Arquivos recebem nome gerado e armazenamento protegido; somente moderação pode baixá-los.
- Fila administrativa protegida por `AdministrationAccess`, com transições `Submitted`, `UnderReview` e `Dismissed`.
- Nenhuma evidência ou descrição é registrada em logs.

## Próximos incrementos

1. Ações administrativas auditadas e suspensão de alvos.
2. Experiência mobile de adoção e denúncia.
3. Política definitiva de retenção de evidências.

A Fase 7 está em andamento.
