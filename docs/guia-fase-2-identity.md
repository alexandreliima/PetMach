# Guia didático — Fase 2: Identity

> Este documento complementa `docs/guia-do-projeto.md`. Deve ser incorporado ao guia principal quando o repositório PetMach estiver aberto como workspace editável.

## Resultado da fase

A Fase 2 implementou a primeira fatia vertical funcional do PetMach: identidade, autenticação, sessões e ciclo básico da conta. Antes desta fase existiam principalmente contratos, interfaces e preparação técnica. Agora existem endpoints, casos de uso, persistência, segurança e cliente mobile para esse fluxo.

## Fluxo de cadastro

```text
1. O cliente envia os dados para POST /api/v1/auth/register.
2. AuthController recebe o RegisterRequest.
3. FluentValidation verifica formato, campos e consentimentos.
4. IdentityService verifica a idade mínima e coordena o cadastro.
5. ASP.NET Core Identity cria a conta com senha armazenada como hash.
6. PetMachDbContext persiste conta e consentimentos versionados.
7. O sistema gera a confirmação de e-mail.
8. Em Development, o e-mail é capturado localmente.
9. O cliente envia userId e token para /api/v1/auth/confirm-email.
10. A conta confirmada pode realizar login.
```

O padrão provisório exige idade mínima de 18 anos e aceite das versões atuais dos termos de uso e da política de privacidade.

## Login e tokens

Após a confirmação do e-mail, o login usa ASP.NET Core Identity para verificar conta e senha. A resposta contém dois tokens:

- **access token:** JWT de curta duração, usado nas chamadas autenticadas da API;
- **refresh token:** segredo de duração maior, usado para renovar a sessão sem solicitar novamente a senha.

O access token dura 15 minutos e o refresh token dura 30 dias por padrão. Esses tempos são configuráveis.

```text
Mobile
  │ envia e-mail e senha
  ▼
AuthController
  │ chama
  ▼
IdentityService
  │ verifica usuário, senha, confirmação e status
  ├── JwtTokenIssuer → cria access token
  └── RefreshToken  → cria sessão persistida
  │
  ▼
API devolve o par de tokens
  │
  ▼
Mobile guarda no SecureStorage
```

## Por que o refresh token é armazenado como hash

O valor original do refresh token é devolvido ao cliente apenas no momento da criação. No banco fica somente seu hash SHA-256.

Isso segue uma ideia semelhante ao armazenamento de senhas: se o banco for acessado indevidamente, o valor armazenado não pode ser usado diretamente para renovar uma sessão.

## Rotação e detecção de reutilização

Cada refresh token é de uso único:

```text
Refresh A é apresentado
        ↓
Refresh A é consumido
        ↓
API cria Access B + Refresh B
```

Se alguém tentar usar o Refresh A novamente, o sistema interpreta isso como possível cópia ou ataque e revoga a família da sessão. Essa proteção também trata concorrência durante a renovação.

## Logout

O logout possui duas partes:

1. A API revoga o refresh token ou sessão correspondente.
2. O aplicativo remove access e refresh token do `SecureStorage`.

Remover apenas os tokens do celular não seria suficiente, pois uma cópia do refresh token poderia continuar válida no servidor.

## Recuperação de senha

O fluxo de recuperação evita revelar se determinado e-mail possui conta. A solicitação responde com `202 Accepted` independentemente da existência do usuário.

Em Development, mensagens são capturadas localmente. Um provedor real de e-mail ainda precisa ser definido antes da produção.

## Anonimização de conta

A exclusão de conta implementada utiliza anonimização imediata:

- dados identificadores são removidos ou substituídos conforme a estratégia atual;
- sessões são revogadas;
- a conta deixa de poder usar tokens existentes.

Regras finais de retenção e textos jurídicos continuam sendo decisões abertas antes da produção.

## Papéis e políticas

O sistema possui papéis e políticas para autorizar operações:

- `TutorAccess`: exige o papel `Tutor`;
- `PartnerAccess`: exige o papel `Partner`;
- `AdministrationAccess`: exige `Administrator` ou `Moderator`.

A autorização é negada por padrão. Isso significa que um endpoint precisa ser explicitamente público ou exigir um usuário/política apropriada.

## Suspensão administrativa

Um administrador ou moderador autorizado pode suspender ou reativar uma conta. A operação:

- exige `AdministrationAccess`;
- altera o estado da conta;
- impede o uso de access tokens pela conta suspensa;
- gera auditoria técnica sem payload sensível.

## Mobile

O núcleo mobile agora possui:

- cliente HTTP para login, refresh e logout;
- `AuthenticationSession` para coordenar a sessão;
- `ITokenStore` para abstrair o armazenamento;
- implementação `SecureTokenStore` usando MAUI `SecureStorage`.

Isso ainda não significa que todas as telas de autenticação estejam concluídas. A integração segura de sessão foi criada; a experiência visual completa permanece como trabalho futuro.

## Banco e migrations

Foram adicionadas:

- `Phase2Identity`;
- `Phase2IdentityAudit`.

Elas representam as alterações necessárias para sessões, consentimentos, estado de conta e auditoria. A verificação informou que não existem mudanças de modelo pendentes.

As migrations ainda precisam ser aplicadas e validadas contra PostgreSQL real. Testes de persistência com Testcontainers permanecem pendentes porque Docker/runtime de containers não estava disponível.

## Arquivos principais

- `backend/src/PetMach.Api/Controllers/AuthController.cs` — endpoints de autenticação e conta;
- `backend/src/PetMach.Api/Controllers/AdministrationIdentityController.cs` — suspensão e reativação;
- `backend/src/PetMach.Infrastructure/Identity/IdentityService.cs` — coordena os casos de uso;
- `backend/src/PetMach.Infrastructure/Identity/JwtTokenIssuer.cs` — gera access tokens;
- `backend/src/PetMach.Infrastructure/Identity/IdentityConfiguration.cs` — configura Identity e autenticação;
- `backend/src/PetMach.Domain/Identity/RefreshToken.cs` — regras da sessão renovável;
- `backend/src/PetMach.Domain/Identity/ConsentRecord.cs` — consentimentos versionados;
- `backend/src/PetMach.Domain/Identity/IdentityAuditLog.cs` — auditoria de Identity;
- `frontend/src/PetMach.Mobile.Core/Identity/AuthenticationSession.cs` — sessão no núcleo mobile;
- `frontend/src/PetMach.Mobile/Identity/SecureTokenStore.cs` — tokens protegidos no aparelho;
- `docs/api/identity.md` — contrato e uso da API de Identity;
- `docs/phase-2-report.md` — relatório técnico da entrega.

## Validação registrada

O relatório da Fase 2 registra:

- 15 projetos compilados;
- build Android aprovado;
- zero erros e zero warnings no gate limpo;
- 22 testes aprovados;
- formatação aprovada;
- nenhuma mudança de modelo pendente nas migrations.

Esses resultados foram conferidos no relatório e a presença da implementação foi inspecionada. Eles não foram executados novamente durante a criação deste guia.

## Estado atualizado do projeto

| Área | Estado atual |
|---|---|
| Fundação da solução | Criada e validada anteriormente |
| Identity e autenticação | Fase 2 implementada |
| Sessão mobile segura | Cliente e SecureStorage implementados; telas completas pendentes |
| PostgreSQL/EF Core para Identity | Modelo e migrations criados; execução em banco real pendente |
| Tutor e cães | Não implementados |
| Saúde e vacinação | Não implementadas |
| Descoberta, likes e matches | Não implementados |
| Chat | Hub vazio; fluxo não implementado |
| Encontros | Não implementados |
| Parceiros, espaços e reservas | Não implementados |
| Adoção, notificações e moderação | Não implementadas |
| Admin | Identity administrativa parcial; painel completo pendente |

## Próxima etapa recomendada

1. Aplicar as migrations em PostgreSQL real.
2. Executar testes de persistência com Testcontainers quando Docker estiver disponível.
3. Realizar aprovação funcional do fluxo de Identity.
4. Iniciar a Fase 3: tutores, cães, fotos e saúde.

