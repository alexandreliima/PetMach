# Guia didático do PetMach

## Objetivo deste documento

Este guia explica o PetMach em linguagem simples: para que serve cada projeto, quem chama quem, onde ficam as regras, como o banco é acessado e o que está ou não implementado. Deve ser atualizado sempre que uma mudança alterar a estrutura, os fluxos, as responsabilidades ou o estado das funcionalidades.

## Visão geral

```text
Aplicativo Mobile
       │ requisição HTTP ou SignalR
       ▼
      API
       │ solicita um caso de uso
       ▼
  Application
       │ utiliza as regras
       ▼
    Domain
       │ leitura ou gravação de dados
       ▼
Infrastructure
       │
       ▼
  PostgreSQL
```

A resposta percorre o caminho inverso:

```text
PostgreSQL → Infrastructure → Application → API → Mobile
```

## Analogia com um restaurante

- **Mobile** é o cliente fazendo o pedido.
- **API** é o garçom que recebe e entrega o pedido.
- **Application** é o gerente que coordena o atendimento.
- **Domain** é o livro com as regras do restaurante.
- **Infrastructure** é a cozinha e os equipamentos.
- **PostgreSQL** é o estoque onde os dados ficam guardados.
- **Contracts** são os formulários padronizados de pedidos e respostas.
- **Admin** é o escritório administrativo.
- **AppHost/Aspire** coordena a abertura dos componentes.
- **ServiceDefaults** fornece configurações técnicas comuns.

## Projetos e responsabilidades

### `frontend/src/PetMach.Mobile`

É o aplicativo instalado no celular. Deverá apresentar login, cadastro, cães, descoberta, matches, chat, reservas e as demais telas do MVP.

O XAML define a aparência das telas. O comportamento principal deve ficar nos ViewModels.

**Estado atual:** fundação MAUI com Shell, página inicial e injeção de dependência básica. Ainda não existe integração real com a API.

### `frontend/src/PetMach.Mobile.Core`

Contém a lógica das telas, principalmente ViewModels. Um ViewModel controla textos, botões e estados como carregando, sucesso, vazio e erro.

```text
MainPage.xaml
      ↓ utiliza
HomeViewModel
```

Separar o ViewModel permite testar o comportamento sem abrir o aplicativo.

**Estado atual:** contém somente o `HomeViewModel` inicial.

### `backend/src/PetMach.Api`

É a porta de entrada do backend. Recebe requisições HTTP do Mobile, Admin ou outro cliente.

```http
POST /api/v1/auth/login
GET  /api/v1/dogs
POST /api/v1/reservations
```

O `Program.cs` configura controllers, autenticação, autorização, erros, OpenAPI, rate limiting, cache, SignalR e health checks. A API deve receber o pedido, identificar o usuário, chamar a Application e devolver uma resposta HTTP. Regras de negócio importantes não devem ficar aqui.

**Estado atual:** infraestrutura HTTP preparada, endpoint básico de sistema e `ChatHub` vazio. Os endpoints do MVP ainda não existem.

### `backend/src/PetMach.Contracts`

Define os formatos dos dados que entram e saem da API, chamados de requests e responses.

```json
{
  "email": "usuario@email.com",
  "password": "senha"
}
```

Isso impede que entidades internas e tabelas sejam expostas diretamente.

**Estado atual:** contratos iniciais de Identity e resposta de informações do sistema.

### `backend/src/PetMach.Application`

Coordena casos de uso, como cadastrar usuário, autenticar, cadastrar cão, curtir perfil ou criar reserva.

Um cadastro deverá aproximadamente:

```text
1. Receber os dados.
2. Validar os campos.
3. Verificar se o e-mail pode ser usado.
4. Criar o usuário.
5. Registrar consentimentos.
6. Iniciar a confirmação de e-mail.
7. Retornar sucesso ou erro controlado.
```

**Estado atual:** registro de validadores, validadores iniciais e interface `IIdentityService`. Ainda não existe a classe que realiza essas operações.

### `backend/src/PetMach.Domain`

É o núcleo das regras do negócio. Não deve depender de tela, HTTP, PostgreSQL ou Entity Framework Core.

Exemplos de regras:

- um tutor não pode curtir o próprio cão;
- um match exige curtida recíproca e não pode ser duplicado;
- uma reserva não pode conflitar com outra;
- um usuário bloqueado não pode enviar mensagens;
- adoção não participa do fluxo comum de likes.

**Estado atual:** Result Pattern, erros básicos, catálogo de módulos e alguns tipos de Identity. A maior parte do domínio ainda não foi implementada.

### `backend/src/PetMach.Infrastructure`

Implementa o contato com tecnologias externas:

- PostgreSQL e Entity Framework Core;
- ASP.NET Core Identity;
- JWT;
- futuros serviços de e-mail, imagens e push notifications.

O `PetMachDbContext` mapeia classes C# para tabelas e executa operações no PostgreSQL.

**Estado atual:** banco e Identity registrados, com migration inicial. O `DbContext` contém principalmente Identity. A autenticação JWT está registrada, mas ainda precisa de configuração e implementação completas.

### `backend/src/PetMach.Admin`

Painel Blazor para consultar usuários e cães, analisar denúncias, suspender contas, administrar parceiros e acompanhar reservas.

**Estado atual:** apenas a fundação Blazor, sem telas administrativas funcionais do MVP.

### `backend/src/PetMach.AppHost`

Orquestra o ambiente com .NET Aspire:

```text
PostgreSQL
    ↓
   API
    ↓
  Admin
```

Facilita iniciar os componentes juntos e acompanhar endpoints, logs, telemetria e saúde.

**Estado atual:** PostgreSQL, API e Admin declarados. A execução dos containers depende de Docker.

### `backend/src/PetMach.ServiceDefaults`

Centraliza configurações compartilhadas de health checks, OpenTelemetry, métricas, rastreamento, service discovery e resiliência HTTP.

### Projetos de testes

- `PetMach.Domain.Tests`: regras puras do domínio.
- `PetMach.Application.Tests`: casos de uso e validações.
- `PetMach.Api.IntegrationTests`: API e integrações.
- `PetMach.Architecture.Tests`: direção das dependências.
- `PetMach.Mobile.Tests`: ViewModels e lógica das telas.

**Estado atual:** poucos testes de fundação. Os fluxos do MVP ainda não estão cobertos.

## Quem pode chamar quem

```text
API ───────────────┐
                   ▼
Infrastructure → Application → Domain
                      │
                      ▼
                  Contracts
```

Regras essenciais:

- Domain não conhece API, banco ou Mobile.
- Application não conhece telas MAUI.
- Mobile e Admin não acessam PostgreSQL diretamente.
- API não devolve entidades do banco diretamente.
- Infrastructure implementa os detalhes técnicos necessários.
- regras de negócio não ficam em controllers, páginas ou migrations.

## Exemplo: futuro cadastro de usuário

```text
1. Usuário preenche o cadastro no Mobile.
2. Mobile envia POST /api/v1/auth/register.
3. Endpoint recebe o RegisterRequest.
4. FluentValidation verifica os campos.
5. O caso de uso de Identity executa as regras.
6. Infrastructure usa Identity e PetMachDbContext.
7. Entity Framework Core grava no PostgreSQL.
8. O caso de uso retorna sucesso ou erro conhecido.
9. A API cria uma resposta HTTP padronizada.
10. O Mobile apresenta o resultado ao usuário.
```

**Estado atual:** contratos, interface e parte das validações existem. Endpoint, implementação, tokens e persistência de consentimentos ainda faltam.

## Estado geral

| Área | Estado atual |
|---|---|
| Fundação da solução | Criada |
| API e middleware básico | Preparados |
| PostgreSQL e EF Core | Preparados para Identity |
| Autenticação completa | Não implementada |
| Tutor e cães | Não implementados |
| Saúde e vacinação | Não implementadas |
| Descoberta e filtros | Não implementados |
| Likes e matches | Não implementados |
| Chat | Hub vazio; fluxo não implementado |
| Encontros | Não implementados |
| Parceiros, espaços e reservas | Não implementados |
| Adoção, notificações e moderação | Não implementadas |
| Mobile | Apenas tela inicial |
| Admin | Apenas fundação Blazor |
| Testes do MVP | Não implementados |

## Regra de manutenção

Revisar este documento quando houver:

- criação, remoção ou renomeação de projeto ou componente importante;
- mudança na direção das chamadas;
- novo endpoint ou fluxo funcional;
- nova tabela, entidade ou integração externa;
- alteração no estado de uma funcionalidade da tabela acima;
- decisão arquitetural que afete a compreensão do sistema.

As explicações devem permanecer simples e tecnicamente verdadeiras. Um recurso apenas registrado ou esboçado nunca deve ser descrito como concluído.

