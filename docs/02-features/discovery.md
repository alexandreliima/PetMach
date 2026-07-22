---
title: PTM-010 — Discovery Experience 2.0
category: Features
type: Feature
status: Working Draft
version: 0.1
owner: PetMatch Mobile
last_reviewed: 2026-07-20
related_documents:
  - README.md
  - ../../mobile-design-system.md
  - ../../api/social.md
  - ../../decisions/ADR-006-geospatial-strategy.md
  - ../../decisions/ADR-007-image-storage.md
---

# PTM-010 — Discovery Experience 2.0

## Summary

PTM-010 é a Epic de evolução da experiência Descobrir do aplicativo Mobile.
O incremento PTM-010A fortalece a fundação existente sem alterar contratos HTTP,
endpoints ou regras de negócio.

## PTM-010A — Discovery Foundation & States

### Scope

O incremento preserva a `CollectionView`, os sete filtros e os endpoints atuais.
Ele implementa:

- estados explícitos `Initial`, `Loading`, `Content`, `Empty` e `Error`;
- lifecycle cancelável entre `OnAppearing` e `OnDisappearing`;
- rejeição de respostas atrasadas por versão de contexto;
- troca segura do pet de origem;
- recarga consistente ao aplicar ou limpar filtros;
- proteção independente de paginação e ações;
- remoção do candidato somente após sucesso;
- feedback visual distinto para sucesso e erro;
- estados visuais, tokens e componentes do Design System;
- semântica e identificadores de automação.

### Architecture

`DiscoveryPage` possui um `DiscoveryPageLifecycle` que cria e descarta o token
associado à presença visual da página. Chamadas consecutivas de `OnAppearing`
compartilham a mesma ativação e não substituem nem descartam o token ativo. Toda
decisão sobre carregamento, estado, filtros, candidatos e ações permanece em
`DiscoveryViewModel`.

O ViewModel cria um CTS próprio, vinculado ao token visual somente no início da
ativação. Seus contextos operacionais dependem desse CTS interno, cuja propriedade
e descarte pertencem ao próprio ViewModel. Em `OnDisappearing`, o coordenador
desativa primeiro o ViewModel e somente depois cancela e descarta o token visual.
Ao desativar, o estado principal retorna para `Initial`; o próximo aparecimento
inicia uma transição previsível por `Loading` até o estado final.

Cada troca de pet ou filtro substitui o contexto corrente. O contexto anterior é
cancelado e recebe uma versão obsoleta. Mesmo quando um cliente de teste ou uma
integração não respeita o cancelamento, a resposta antiga não pode modificar a
coleção ou o estado atual.

### States and recovery

- `Initial`: a página ainda não iniciou a ativação.
- `Loading`: cães ou candidatos estão sendo carregados.
- `Content`: existem candidatos disponíveis.
- `Empty`: não existem candidatos ou o tutor ainda não cadastrou um pet.
- `Error`: o carregamento inicial ou a recarga falhou e pode ser repetido.

Erros de like, pass, bloqueio e paginação não descartam o conteúdo carregado.
Essas falhas usam feedback de erro recuperável. A nova tentativa limpa mensagens
anteriores antes de buscar novamente.

### Cancellation and concurrency

- sair da página cancela o contexto;
- `OnAppearing` duplicado reutiliza a ativação vigente;
- reaparecer cria um novo token;
- mudar o pet cancela a consulta anterior;
- aplicar ou limpar filtros reinicia a primeira página;
- uma paginação já em andamento rejeita outra paginação;
- uma ação de candidato já em andamento rejeita like, pass ou bloqueio
  concorrente;
- paginação e ações de candidato são mutuamente exclusivas;
- o token interno do comando não substitui uma ação em andamento;
- o candidato é removido somente depois da confirmação da API.

### Accessibility

A página identifica controles relevantes com `AutomationId`, anuncia loading,
vazio, erro e feedback, descreve a fotografia e fornece nomes textuais para
curtir, ignorar e bloquear. As ações utilizam o touch target do Design System e
não dependem apenas de símbolos ou cor.

### Test evidence

Os 28 testes específicos de Discovery em `DiscoveryViewModelTests` e
`DiscoveryPageLifecycleTests` cobrem carregamento inicial, conteúdo, vazio, erro,
retry, ativação e `OnAppearing` duplicados, cancelamento, reaparecimento, troca de
pet, resposta atrasada, filtros, paginação, like, pass, bloqueio, exclusão mútua
entre paginação e ações, preservação do candidato, feedback de match e limpeza
de estado.

## Known limitations

- A paginação continua baseada em página/offset. Como likes e passes removem
  candidatos da consulta, páginas posteriores ainda podem pular perfis. A
  correção por cursor ou estratégia equivalente exige evolução de backend e
  contrato fora da PTM-010A.
- A URL da foto principal continua protegida por autenticação, mas o controle
  `Image` não utiliza o cliente autenticado. O carregamento autenticado, cache,
  fallback de rede e preload pertencem à PTM-010B.
- A descoberta continua com vários cards em `CollectionView`. Card único,
  buffer, swipe e animações serão tratados em incremento posterior.
- O match permanece como feedback textual. Não há modal neste incremento.
- Não existe perfil público real por `petId`; essa capacidade está planejada
  para PTM-010D.
- Distância e compatibilidade permanecem fora do escopo.

## Planned increments

- PTM-010B — card, buffer e carregamento autenticado de imagens;
- PTM-010C — interações, swipe, animações e feedback de match;
- PTM-010D — perfil público baseado em dados reais;
- PTM-010E — filtros avançados, localização protegida e distância.
