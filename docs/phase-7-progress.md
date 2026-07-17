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
- Migration `Phase7ModerationReports` aplicada e validada no PostgreSQL real.
- Formatação e build integral aprovados, com 129 testes aprovados, incluindo concorrência no PostgreSQL.
- Nenhum erro; permanecem somente avisos `NU1900` da auditoria NuGet offline.

## Incremento 4 — ações administrativas auditadas

- Ação exige denúncia previamente colocada em revisão.
- A ação deve corresponder ao alvo: suspender usuário, cão ou publicação de adoção.
- Suspensão de usuário reutiliza revogação de sessões e auditoria de identidade.
- Cada denúncia aceita no máximo uma ação administrativa, garantida por constraint única.
- Auditoria registra moderador, ação, tipo do alvo, identificador e instante UTC, sem justificativa ou evidência sensível.
- Após sucesso, a denúncia passa para `Actioned`.
- Migration `Phase7ModerationActions` aplicada e validada no PostgreSQL real.
- Formatação e build integral aprovados, com 130 testes aprovados, incluindo PostgreSQL real.
- Nenhum erro; permanecem somente avisos `NU1900` da auditoria NuGet offline.

## Incremento 5 — adoção e denúncia no mobile

- Tela única reúne catálogo, publicação de cão próprio, candidatura e acompanhamento.
- Aceites de termos permanecem explícitos para publicação e candidatura.
- Proprietário pode suspender a própria publicação e candidato pode retirar pedidos elegíveis.
- Uma publicação selecionada pode ser denunciada com motivo controlado e descrição objetiva.
- Estados de ação são derivados dos contratos do servidor; o mobile não contorna ownership ou transições.
- Formatação, build integral e Android aprovados, com 136 testes aprovados, incluindo PostgreSQL real.
- Nenhum erro; permanecem somente avisos `NU1900` da auditoria NuGet offline.

## Incremento 6 — dashboard administrativo

- Login server-side cria cookie HTTP-only somente após validar JWT e papel `Administrator` ou `Moderator`.
- Token da API permanece no cookie criptografado do servidor e não é entregue ao JavaScript.
- Todas as mutações usam POST, autorização por política e validação antiforgery.
- Dashboard apresenta fila, descrição, evidências protegidas e ações compatíveis com o alvo.
- Download de evidência ocorre por proxy autenticado, sem expor storage key ou token.
- Sessão administrativa expira junto com o access token da API.
- Formatação e build integral aprovados, com 137 testes aprovados, incluindo PostgreSQL real.
- Nenhum erro; permanecem somente avisos `NU1900` da auditoria NuGet offline.

## Incremento 7 — evidências mobile e revisão de segurança

- Denúncia mobile permite evidência opcional JPEG, PNG ou PDF de até 5 MB.
- Limite HTTP considera overhead multipart, enquanto a validação de conteúdo mantém o limite real de 5 MB.
- Tipo declarado pelo dispositivo não é confiado; a API detecta a assinatura real do arquivo.
- Login administrativo exige papel válido, cookie HTTP-only, `SameSite=Strict` e expiração junto ao access token.
- Mutações administrativas usam antiforgery e evidências passam por proxy autenticado.
- `.env`, tokens, senhas, storage keys, descrições e evidências não são registrados em logs.
- Formatação, build integral e Android aprovados, com 137 testes aprovados, incluindo PostgreSQL real.
- Nenhum erro; permanecem somente avisos `NU1900` da auditoria NuGet offline.

## Pendências para produção

- Definir prazo jurídico de retenção e eliminação de denúncias e evidências.
- Configurar object storage protegido com criptografia, backup e auditoria de acesso.
- Definir matriz final de motivos e ações permitidas por `Moderator` e `Administrator`.
- Substituir a chave efêmera de proteção de cookies do Admin por key ring persistente e protegido.

## Próximos incrementos

1. Fase 8 — consolidação mobile, acessibilidade e estados de tela.
2. Política definitiva de retenção antes de produção.

A Fase 7 está funcionalmente concluída.
