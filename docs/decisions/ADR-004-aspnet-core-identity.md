# ADR-004 — ASP.NET Core Identity

- Status: Aceita
- Data: 2026-07-13

## Contexto

O MVP requer senha segura, confirmação, recuperação, papéis, bloqueio e preparação para Google/Apple, além de JWT e refresh rotativo para o mobile.

## Decisão

Usar ASP.NET Core Identity para credenciais, usuário e papéis. Emitir access JWT de curta duração e refresh token opaco armazenado somente como hash, com rotação, família, revogação e detecção de reuse. Autorização usa políticas e checagem de recurso.

## Consequências

- Evita implementar primitivas críticas de autenticação do zero.
- Fluxos JWT/refresh e LGPD continuam sendo responsabilidade da aplicação e exigem testes próprios.
- Provedores externos serão adaptadores futuros, sem alterar a identidade interna do usuário.
