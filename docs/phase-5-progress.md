# Progresso — Fase 5

Data de início: 2026-07-14

## Incremento 1 — conversas e mensagens de texto

- Uma conversa única nasce atomicamente com cada novo match.
- Somente participantes de match ativo e sem bloqueio podem listar, entrar ou enviar mensagens.
- Mensagens textuais imutáveis, limitadas a 2.000 caracteres e persistidas antes da publicação em tempo real.
- Histórico paginado pela API, com ordenação determinística.
- SignalR autoriza a entrada no grupo da conversa e publica `MessageReceived` depois da persistência.
- Telas mobile iniciais para lista de conversas, histórico e envio.
- Unmatch e bloqueio interrompem imediatamente o acesso, preservando dados conforme default provisório documentado.

## Gate remanescente

- Executar cenários autenticados de isolamento, bloqueio, paginação e transições sobre PostgreSQL real via Testcontainers quando Docker estiver disponível.
- Aplicar e validar as migrations em uma base descartável antes de declarar a fase concluída pelo Definition of Done.

## Incremento 2 — SignalR no aplicativo

- Cliente MAUI autenticado usando o token obtido pela sessão segura.
- Reconexão automática com atrasos progressivos e nova entrada no grupo autorizado.
- Ciclo de conexão vinculado à tela de chat, encerrando a conexão ao sair.
- Deduplicação por identificador entre histórico REST, resposta de envio e evento `MessageReceived`.
- Atualizações da coleção encaminhadas ao contexto da interface.

## Incremento 3 — estado de leitura

- Marcador de leitura individual por conversa e participante.
- Avanço monotônico pelo instante persistido da mensagem; requisições antigas não retrocedem o estado.
- Contagem de mensagens não lidas na lista de conversas.
- Atualização automática ao abrir o histórico ou receber uma mensagem em tempo real.
- Evento SignalR `ConversationRead` publicado depois da persistência.
- Ownership e disponibilidade da conversa validados antes da atualização.

## Incremento 4 — propostas de encontro

- Propostas vinculadas a matches ativos, com data/hora futura, nome público do local e observação opcional.
- Estados `Proposed`, `Accepted`, `Declined` e `Cancelled` com transições explícitas.
- Somente o outro participante pode aceitar ou recusar; ambos podem cancelar propostas pendentes ou aceitas.
- Unmatch e bloqueio impedem novas transições.
- Nenhuma coordenada exata é armazenada ou retornada.
- Tela mobile para listar, propor e responder encontros.

## Incremento 5 — notificações de encontros

- A caixa interna agora diferencia tipos e referências de match e encontro.
- Nova proposta notifica o outro participante.
- Aceite, recusa e cancelamento notificam somente o participante oposto à ação.
- Notificações são persistidas na mesma transação da criação ou mudança de estado.
- Índices de unicidade impedem a repetição do mesmo evento para o mesmo destinatário.

## Revisão de encerramento

| Critério | Estado |
|---|---|
| Conversas e texto persistente | Implementado |
| Histórico paginado | Implementado |
| Autorização, bloqueio e unmatch | Implementado; teste PostgreSQL pendente |
| SignalR, reconexão e deduplicação | Implementado e testado no núcleo mobile |
| Leitura por participante | Implementado; teste PostgreSQL pendente |
| Propostas e transições de encontro | Implementado; teste PostgreSQL pendente |
| Telas mobile | Implementadas |
| Migrations | Geradas; aplicação em base descartável pendente |

O escopo funcional da Fase 5 está implementado. A fase permanece em validação, sem declaração de conclusão, até o gate PostgreSQL/Testcontainers.
