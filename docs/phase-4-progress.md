# Progresso — Fase 4

Data de início: 2026-07-14

## Incremento 1 — descoberta, likes, matches e bloqueios

- Descoberta autenticada e paginada por um cão de origem escolhido pelo tutor.
- Filtros por sexo, porte, raça, energia, objetivo, castração e vacinação atualizada.
- Exclusão automática de cães próprios, suspensos, ocultos, já curtidos, ignorados ou pertencentes a usuários bloqueados.
- Like direcionado entre cães, com proibição de auto-like e de interação entre cães do mesmo tutor.
- Match criado somente quando existe like recíproco.
- Ordenação canônica e índice único impedem matches duplicados.
- Ação de ignorar remove o perfil das próximas páginas daquele cão.
- Bloqueio bilateral oculta perfis e encerra matches ativos entre os tutores.
- Desfazer match preserva o histórico e marca o encerramento em UTC.
- Foto principal disponibilizada apenas por endpoint autenticado, sem expor a chave de armazenamento.
- Telas mobile iniciais de descoberta e lista de matches conectadas à API.
- Fluxo público corrigido: “Conhecer o PetMach” abre uma apresentação institucional sem liberar a área autenticada.
- Migration `Phase4DiscoveryMatches`.

## Privacidade

- Nenhuma coordenada exata é retornada.
- A descoberta exibe somente a região autorizada pelo tutor.
- O filtro de distância permanece desativado até a captura protegida da localização e a granularidade exibida serem definidas.

## Incremento 2 — filtros e paginação mobile

- Tela de descoberta com filtros por sexo, porte, raça, energia, objetivo, castração e vacinação atualizada.
- Contrato mobile tipado para enviar filtros e página à API.
- Paginação incremental sem recarregar candidatos já apresentados.
- Ação para limpar filtros e reiniciar a consulta.
- Fluxos corrigidos para cadastro concluído seguir ao login e perfil salvo voltar ao início.

## Próximos incrementos

1. Persistir localização protegida e calcular distância exclusivamente no servidor.
2. Executar cenários concorrentes de likes e matches no PostgreSQL via Testcontainers quando Docker estiver disponível.

## Incremento 3 — notificações internas de match

- Duas notificações persistentes são criadas atomicamente com cada novo match, uma para cada tutor.
- Caixa interna autenticada limitada às 100 notificações mais recentes do próprio tutor.
- Marcação de leitura monotônica e protegida por ownership.
- Tela mobile de notificações conectada à API.
- Migration `Phase4MatchNotifications`.

A Fase 4 está em andamento.
