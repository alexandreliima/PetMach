# ADR-005 — SignalR para tempo real

- Status: Aceita
- Data: 2026-07-13

## Contexto

O chat exige entrega em tempo real, mas mensagens também precisam persistência, paginação, autorização e funcionamento após reconexão.

## Decisão

Usar SignalR como canal de entrega, não como fonte de verdade. Persistir a mensagem autorizada no PostgreSQL antes de publicá-la aos participantes. Histórico é sempre consultado pela API paginada. O MVP opera sem backplane Redis em instância única.

## Consequências

- Reconexão não perde o histórico confirmado.
- Escala horizontal futura exigirá backplane ou serviço gerenciado e novo desenho de entrega.
- Hub e endpoints compartilham o mesmo caso de uso/autorização para evitar regras divergentes.
