# Arquitetura proposta

## Direção

O PetMach será um monólito modular implantável como uma API principal, um painel Admin separado e um cliente mobile. O backend compartilha processo e banco PostgreSQL, mas mantém limites de módulos verificáveis. A separação `backend/` e `frontend/` é física; contratos HTTP versionados são a fronteira entre ambos.

```mermaid
flowchart LR
    Mobile["PetMach Mobile (.NET MAUI)"] -->|"HTTPS /api/v1 + SignalR"| API["PetMach API"]
    Admin["PetMach Admin (Blazor)"] -->|"Aplicação e políticas"| API
    API --> Modules["Módulos de negócio"]
    Modules --> DB[("PostgreSQL")]
    Modules --> Storage["Storage de imagens"]
    AppHost[".NET Aspire AppHost"] -. orquestra .-> API
    AppHost -. orquestra .-> Admin
    AppHost -. orquestra .-> DB
```

## Estrutura planejada

```text
PetMach/
├── AGENTS.md
├── README.md
├── PetMach.code-workspace
├── PetMach.slnx
├── global.json
├── Directory.Build.props
├── Directory.Packages.props
├── docker-compose.yml
├── .editorconfig
├── .gitignore
├── .github/workflows/ci.yml
├── backend/
│   ├── src/
│   │   ├── PetMach.Api/
│   │   ├── PetMach.Domain/
│   │   ├── PetMach.Application/
│   │   ├── PetMach.Infrastructure/
│   │   ├── PetMach.Contracts/
│   │   ├── PetMach.Admin/
│   │   ├── PetMach.AppHost/
│   │   └── PetMach.ServiceDefaults/
│   └── tests/
│       ├── PetMach.Domain.Tests/
│       ├── PetMach.Application.Tests/
│       ├── PetMach.Api.IntegrationTests/
│       └── PetMach.Architecture.Tests/
├── frontend/
│   ├── src/PetMach.Mobile.Core/
│   ├── src/PetMach.Mobile/
│   └── tests/PetMach.Mobile.Tests/
└── docs/
    ├── decisions/
    ├── diagrams/
    ├── api/
    ├── database/
    └── product/
```

Os projetos de backend usam pastas de primeiro nível por módulo (`Identity`, `Tutors`, `Dogs` etc.) em Domain, Application e Infrastructure. Isso preserva as quatro dependências arquiteturais sem criar dezenas de assemblies no início. Testes arquiteturais impedem dependências proibidas. Se o acoplamento crescer, um módulo pode ser extraído para assemblies próprios sem alterar seus contratos externos.

## Regra de dependências

```mermaid
flowchart LR
    Api --> Application
    Api --> Infrastructure
    Admin --> Application
    Admin --> Infrastructure
    Infrastructure --> Application
    Infrastructure --> Domain
    Application --> Domain
    Application --> Contracts
    Mobile --> Contracts["Contratos HTTP gerados/compartilhados sem domínio"]
```

- Domain não depende de Application, Infrastructure, API ou UI.
- Application orquestra casos de uso e portas; não conhece EF Core ou transporte HTTP.
- Infrastructure implementa persistência, Identity, storage, relógio e integrações.
- API autentica, autoriza, valida transporte e converte resultados em Problem Details.
- Contracts contém DTOs públicos estáveis, paginação e eventos de integração deliberados; nunca entidades.
- Mobile consome a API e não referencia projetos internos do backend. Compartilhamento de contratos deve ocorrer por cliente OpenAPI ou pacote dedicado sem lógica de domínio.

## Módulos

| Módulo | Responsabilidade | Dependências permitidas relevantes |
|---|---|---|
| SharedKernel | IDs, Result, erros, abstrações mínimas e eventos | Nenhuma regra de módulo |
| Identity | credenciais, papéis, consentimento, tokens e ciclo da conta | Notifications, Audit por portas |
| Tutors | perfil do tutor e privacidade | Identity por identificador |
| Dogs | perfil, fotos, raça, preferências e status | Tutors |
| Health | vacina, vermifugação e saúde protegida | Dogs |
| Discovery | candidatos, filtros e distância aproximada | Dogs, Tutors, Health, Moderation |
| Matches | likes, reciprocidade, match e unmatch | Dogs, Moderation, Notifications |
| Chat | conversa, mensagens, leitura e SignalR | Matches, Moderation, Notifications |
| Meetings | propostas e transições de encontro | Matches, Partners, Reservations |
| Partners | estabelecimento, representante, serviços e horários | Identity, Administration |
| Spaces | espaço, recursos, regras e slots | Partners |
| Reservations | disponibilidade, participantes, conflito e histórico | Spaces, Dogs, Notifications |
| Adoption | perfis e histórico independente de likes | Dogs, Moderation, Notifications |
| Notifications | caixa interna e porta para push futuro | Identity |
| Moderation | bloqueios, denúncias, evidências e ações | Identity, Audit |
| Administration | consultas administrativas, parâmetros e políticas | Todos por casos de uso públicos |

As dependências da tabela são relações de negócio, não autorização para ler tabelas internas. Administração usa projeções/casos de uso explícitos.

## Persistência

- Um PostgreSQL por implantação e um `DbContext` inicial, com mapeamentos agrupados por módulo e schemas lógicos por módulo quando isso não prejudicar Identity/migrations.
- Migration única e ordenada no projeto Infrastructure durante o MVP.
- Constraints e índices garantem invariantes concorrentes que validação de aplicação não consegue garantir.
- `Guid` gerado pela aplicação como identificador uniforme; `DateTimeOffset` UTC para eventos.
- Localização começa com tipos/consultas PostgreSQL adequados e cálculo no servidor. PostGIS somente após validação de necessidade e disponibilidade operacional.
- Redis não é fonte de verdade e não entra no caminho crítico inicial.

## API e erros

- Prefixo `/api/v1`, OpenAPI nativo e paginação por cursor onde há fluxo cronológico; paginação por página somente em consultas administrativas adequadas.
- Requests possuem validadores e mapeamento explícito para comandos/casos de uso.
- Erros seguem RFC 9457 Problem Details com códigos estáveis (`dog.not_found`, `reservation.conflict`).
- Autorização combina papéis e políticas baseadas em recurso; a checagem de participante permanece no caso de uso.
- Idempotência será aplicada a confirmações de reserva e outras escritas suscetíveis a repetição do cliente.

## Fluxos críticos

### Autenticação

```mermaid
sequenceDiagram
    participant M as Mobile
    participant A as API
    participant I as Identity
    participant D as PostgreSQL
    M->>A: login
    A->>I: validar credenciais e conta
    I->>D: consultar usuário
    I-->>A: access token + refresh token rotativo
    A-->>M: tokens e expirações
    M->>A: refresh token
    A->>I: validar, revogar anterior e criar sucessor
    I->>D: transação de rotação
    A-->>M: novo par de tokens
```

### Match

```mermaid
sequenceDiagram
    participant U as Tutor
    participant A as API
    participant M as Matches
    participant D as PostgreSQL
    U->>A: curtir cão alvo
    A->>M: Like(cão origem, cão alvo)
    M->>M: validar dono, bloqueio e status
    M->>D: inserir like único
    M->>D: procurar like inverso
    alt recíproco
        M->>D: inserir match único e conversa
        M-->>A: match criado
    else não recíproco
        M-->>A: like registrado
    end
```

### Reserva

```mermaid
sequenceDiagram
    participant U as Tutor
    participant A as API
    participant R as Reservations
    participant D as PostgreSQL
    U->>A: confirmar reserva
    A->>R: CreateReservation
    R->>D: iniciar transação
    R->>D: verificar slot e conflitos
    R->>D: inserir reserva com token de concorrência
    alt disponível
        D-->>R: commit
        R-->>A: confirmada/pendente
    else conflito
        D-->>R: rollback/constraint violation
        R-->>A: reservation.conflict
    end
```

## Observabilidade

Aspire Service Defaults configura OpenTelemetry, health checks e service discovery. Cada requisição recebe correlation/trace ID. Métricas evitam dimensões com usuário, coordenada ou conteúdo de mensagem. Logs estruturados registram ação, resultado e identificadores técnicos necessários, nunca segredos ou saúde sensível.

## Evolução

Os primeiros candidatos à extração futura são Chat/Notifications (escala de conexões) e Reservations (isolamento transacional/comercial). Extração exige métrica, contrato de integração, outbox/idempotência e ADR; não faz parte do MVP.
