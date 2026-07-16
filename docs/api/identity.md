# API de identidade — v1

Todos os corpos e respostas usam JSON. Erros usam Problem Details e incluem `code` e `traceId`.

## Fluxo principal

1. `POST /api/v1/auth/register` cria uma conta pendente e exige data de nascimento e consentimentos vigentes.
2. Em Development, leia a mensagem capturada em `backend/src/PetMach.Api/.dev-emails/` e envie `userId` e `token` para `POST /api/v1/auth/confirm-email`.
3. `POST /api/v1/auth/login` retorna access token de 15 minutos e refresh token de 30 dias.
4. `POST /api/v1/auth/refresh` consome o refresh atual e retorna um novo par. Reutilizar um token consumido revoga sua família inteira.
5. `POST /api/v1/auth/logout` revoga o refresh informado. O cliente sempre remove a sessão local.

## Segurança

- Senhas são armazenadas somente pelos hashers do ASP.NET Core Identity.
- Refresh tokens são aleatórios e persistidos apenas como SHA-256; o valor puro só é retornado ao cliente.
- JWTs usam HMAC SHA-256, emissor/audiência validados e chave efêmera em Development.
- Fora de Development, `Identity__SigningKey` é obrigatória e deve vir de secret store ou variável de ambiente.
- Uma conta suspensa ou anonimizada é rejeitada durante a validação de cada access token.
- Recuperação de senha sempre retorna `202 Accepted`, exista ou não a conta.

## Políticas

- Autorização é negada por padrão.
- `TutorAccess` requer papel `Tutor`.
- `PartnerAccess` requer papel `Partner`.
- `AdministrationAccess` requer `Administrator` ou `Moderator`.
- Suspensão/reativação exige `AdministrationAccess` e produz auditoria sem payload sensível.

## Defaults provisórios

Idade mínima 18 anos, versões de termos e privacidade `2026-07-14`, papel inicial apenas `Tutor` e anonimização imediata. Consulte `docs/open-questions.md` antes de produção.
