# API social — v1

Todos os endpoints exigem `TutorAccess`. Recursos de outro tutor retornam `404` para não confirmar sua existência.

## Descoberta e matches

- `GET /api/v1/discovery`: candidatos paginados e filtrados para um cão ativo do tutor.
- `POST /api/v1/dogs/{targetDogId}/likes`: registra like e cria match/conversa quando há reciprocidade.
- `POST /api/v1/dogs/{targetDogId}/passes`: ignora o perfil para o cão de origem.
- `GET /api/v1/matches`: lista matches ativos do tutor.
- `DELETE /api/v1/matches/{matchId}`: encerra o match.

## Chat

- `GET /api/v1/chat/conversations`: lista conversas de matches ativos, incluindo contagem de mensagens não lidas.
- `GET /api/v1/chat/conversations/{id}/messages?page=1&pageSize=30`: histórico paginado, com página entre 1 e 50 itens.
- `POST /api/v1/chat/conversations/{id}/messages`: persiste texto de 1 a 2.000 caracteres.
- `PUT /api/v1/chat/conversations/{id}/read`: avança o marcador de leitura para `messageId`.
- `/hubs/chat`: publica `MessageReceived` e `ConversationRead`; `JoinConversation` revalida participante, match e bloqueios.

SignalR é somente canal de entrega. A API e o PostgreSQL permanecem como fonte de verdade.

## Encontros

- `GET /api/v1/meetings`: lista propostas dos matches do tutor.
- `POST /api/v1/meetings`: cria proposta futura com `matchId`, `scheduledAtUtc`, `placeName` e `notes` opcional.
- `PUT /api/v1/meetings/{id}/accept`: aceita como destinatário.
- `PUT /api/v1/meetings/{id}/decline`: recusa como destinatário.
- `PUT /api/v1/meetings/{id}/cancel`: cancela proposta pendente ou aceita.

Nenhum contrato recebe ou retorna coordenadas. Bloqueio ou unmatch interrompe chat e transições de encontro.
