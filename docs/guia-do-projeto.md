# Guia didático do PetMach

## Para que serve este guia

Este documento explica a arquitetura atual em linguagem direta. Para comandos,
consulte [Operação e execução](operations.md); para detalhes verificáveis,
consulte [Estado técnico atual](current-state.md).

## O produto

O PetMach conecta tutores e pets. Um tutor cria sua conta e os perfis dos
animais, descobre outros pets, registra likes e forma matches recíprocos. A
partir do match, os tutores podem conversar e combinar encontros.

O produto também possui:

- saúde e vacinação protegidas;
- espaços de parceiros e reservas;
- adoção responsável separada dos likes;
- denúncias e moderação;
- painel administrativo.

## O caminho de uma operação

```text
Mobile ou Admin
       │ HTTP / SignalR
       ▼
      API
       │ coordena
       ▼
  Application
       │ aplica
       ▼
    Domain
       ▲
       │ implementa persistência e integrações
Infrastructure
       │
       ▼
  PostgreSQL
```

- **Mobile** é a experiência do tutor.
- **Admin** é a experiência de moderadores e administradores.
- **API** autentica, autoriza e traduz HTTP para casos de uso.
- **Application** define portas, validadores e operações.
- **Domain** mantém invariantes que não dependem de tecnologia.
- **Infrastructure** implementa EF Core, PostgreSQL, Identity, JWT e storage.
- **Contracts** define requests e responses públicos.
- **ServiceDefaults** configura saúde, telemetria, resiliência e descoberta.
- **AppHost** inicia o ambiente de desenvolvimento pelo Aspire.

## Projetos

### `backend/src/PetMach.Api`

Expõe os controllers em `/api/v1`, o hub `/hubs/chat`, OpenAPI em Development e
os endpoints `/health/live` e `/health/ready`. A readiness inclui o acesso ao
PostgreSQL.

### `backend/src/PetMach.Domain`

Contém regras de identidade, tutores, pets, saúde, descoberta, matches, chat,
encontros, parceiros, reservas, adoção, notificações e moderação.

### `backend/src/PetMach.Application`

Contém interfaces dos serviços e validadores. Ela coordena os casos de uso sem
conhecer controllers, páginas ou detalhes do EF Core.

### `backend/src/PetMach.Contracts`

Contém os contratos HTTP versionados. Entidades do domínio não são retornadas
diretamente pela API.

### `backend/src/PetMach.Infrastructure`

Contém `PetMachDbContext`, serviços dos módulos, ASP.NET Core Identity, emissão
JWT e integrações. As 18 migrations EF Core também ficam nesse projeto.

### `backend/src/PetMach.Admin`

É uma aplicação Blazor Interactive Server. Autentica pela API, exige papel
administrativo, mantém cookie HTTP-only e permite operar a fila de moderação
com antiforgery.

### `backend/src/PetMach.AppHost`

Declara PostgreSQL, API e Admin no .NET Aspire. Referências entre recursos
fornecem connection string e service discovery no ambiente de desenvolvimento.

### `frontend/src/PetMach.Mobile.Core`

Contém ViewModels, clientes HTTP, sessão e abstrações de navegação. Por não
depender da plataforma MAUI, pode ser testado em `net10.0`.

### `frontend/src/PetMach.Mobile`

Contém páginas XAML, integrações Android/iOS, `SecureStorage`, cliente SignalR e
a implementação de navegação.

## Exemplo: login e sessão

```text
1. LoginPage envia e-mail e senha pelo AuthApiClient.
2. A API valida credenciais e estado da conta.
3. A API devolve access token e refresh token.
4. AuthenticationSession grava os tokens pelo ITokenStore.
5. SecureTokenStore usa SecureStorage no dispositivo.
6. RootNavigationService cria uma nova AppShell autenticada.
7. Chamadas protegidas anexam o access token.
8. Se uma chamada recebe 401, uma única renovação compartilhada é tentada.
9. A requisição é repetida no máximo uma vez.
10. Logout ou falha definitiva limpa a sessão, encerra conexões e cria uma nova raiz pública.
```

Login e refresh usam um cliente separado das chamadas protegidas, portanto não
entram em repetição recursiva.

## Exemplo: match

```text
1. O tutor escolhe um pet de origem.
2. Mobile consulta candidatos autorizados na API.
3. O tutor registra um like.
4. A API valida ownership, estado e bloqueios.
5. PostgreSQL grava o like com constraints de unicidade.
6. Se houver like inverso, nasce um único match e uma conversa.
7. Os dois tutores recebem notificações.
```

## Exemplo: reserva

```text
1. O tutor escolhe pet, espaço e disponibilidade.
2. A API valida ownership e período.
3. A reserva começa pendente.
4. O parceiro confirma ou cancela.
5. Constraint PostgreSQL impede duas reservas ativas na mesma disponibilidade.
6. O histórico preserva ator, estado e instante UTC.
```

Pagamento permanece presencial/informativo; não existe pagamento real no MVP.

## Banco e migrations

PostgreSQL é a fonte de verdade. O schema é produzido exclusivamente pelas 18
migrations reais; não por `EnsureCreated` ou por um provider alternativo.

No Docker Compose:

```text
PostgreSQL saudável
        ↓
Migrator aplica migrations e termina com exit code 0
        ↓
API inicia e fica saudável
        ↓
Admin inicia e fica saudável
```

Em execução local ou Aspire, as migrations são aplicadas explicitamente com
`dotnet ef database update`.

## Testes

- domínio: invariantes puras;
- aplicação: validações e DI;
- arquitetura: dependências permitidas;
- integração: endpoints, autenticação e persistência;
- mobile: ViewModels, navegação, sessão, refresh e clientes.

Os cinco testes PostgreSQL iniciam `postgres:18.0-alpine` automaticamente,
aplicam as 18 migrations, isolam dados e validam concorrência. Sem Docker, a
fixture falha explicitamente; não há falso verde.

## Estado funcional

| Área | Estado no repositório |
|---|---|
| Identidade e sessão | Implementadas |
| Tutor, pets e saúde | Implementados |
| Descoberta, likes e matches | Implementados |
| Chat, leitura e encontros | Implementados |
| Parceiros, espaços e reservas | Implementados |
| Adoção e candidaturas | Implementadas |
| Denúncias, moderação e Admin | Implementados |
| Persistência PostgreSQL | 18 migrations e Testcontainers validados |
| Docker Compose | PostgreSQL, migrator, API e Admin validados |
| Mobile Android | Build e execução local suportados |
| Mobile iOS | Projeto condicional; validação exige macOS |

“Implementado” não significa pronto para produção. Ainda são necessários
políticas jurídicas definitivas, storage protegido de produção, backup/restore,
validação iOS, segurança, performance, acessibilidade e preparação de release.

## Onde continuar

- [Estado técnico atual](current-state.md);
- [Arquitetura](architecture.md);
- [Diagramas](diagrams.md);
- [Operação e execução](operations.md);
- [Testes](testing.md);
- [Perguntas em aberto](open-questions.md);
- [Riscos](risks.md).
