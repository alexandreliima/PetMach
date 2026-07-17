# Contexto de continuidade — PetMach

## Papel do Codex

Neste projeto, o Codex deve atuar como **engenheiro de software sênior especialista em .NET e C#**, com experiência em:

- .NET 10 e C#;
- ASP.NET Core Web API;
- .NET MAUI;
- Blazor;
- Entity Framework Core e PostgreSQL;
- .NET Aspire;
- Clean Architecture, DDD e Vertical Slice Architecture;
- segurança, LGPD, testes e observabilidade.

O trabalho deve ser colaborativo e didático: antes de mudanças relevantes, explicar o que está acontecendo, preservar o código existente e validar cada incremento com formatação, build e testes. Nunca declarar que algo funciona sem executar as validações correspondentes.

## Objetivo do produto

O PetMach nasceu de um prompt chamado **DogMatch**. É uma plataforma mobile de socialização, experiências e serviços para cães e seus tutores. O match é uma ferramenta de descoberta; o valor principal do produto está em encontros seguros, reservas e integração com estabelecimentos parceiros.

O MVP previsto inclui:

- autenticação e conta do tutor;
- perfis de tutores e cães;
- saúde e vacinação com acesso controlado;
- descoberta, filtros, likes e matches;
- chat após match;
- encontros;
- parceiros, espaços e reservas;
- área separada para adoção responsável;
- notificações internas;
- bloqueios, denúncias e moderação;
- aplicativo .NET MAUI e painel administrativo Blazor.

Não fazem parte do MVP: reprodução/cruzamento, pagamento real, chamadas, áudio ou vídeo.

## Repositório

Caminho local:

```text
C:\Users\alima\Desktop\Meus Doc\Projetos IA\PetMach
```

O repositório possui um `AGENTS.md` na raiz, que deve ser lido antes de qualquer alteração. Ele determina as regras de arquitetura, segurança, testes e Definition of Done.

## Arquitetura adotada

O sistema começa como um **monólito modular**, separado fisicamente em backend e frontend.

Fluxo geral:

```text
.NET MAUI
    ↓ HTTP / SignalR
ASP.NET Core API
    ↓
Application
    ↓
Domain
    ↑
Infrastructure → PostgreSQL / ASP.NET Core Identity
```

Principais projetos:

- `backend/src/PetMach.Domain`: regras e tipos de domínio;
- `backend/src/PetMach.Application`: casos de uso, interfaces e validações;
- `backend/src/PetMach.Contracts`: contratos HTTP;
- `backend/src/PetMach.Infrastructure`: EF Core, PostgreSQL, Identity e autenticação;
- `backend/src/PetMach.Api`: API, middleware, controllers e SignalR;
- `backend/src/PetMach.Admin`: painel Blazor;
- `backend/src/PetMach.AppHost`: orquestração Aspire;
- `backend/src/PetMach.ServiceDefaults`: health checks, telemetria e service discovery;
- `frontend/src/PetMach.Mobile`: interface MAUI;
- `frontend/src/PetMach.Mobile.Core`: ViewModels testáveis;
- `backend/tests` e `frontend/tests`: projetos de testes.

## Estado encontrado em 14 de julho de 2026

O projeto avançou além da Fase 0 e possui uma fundação técnica correspondente à Fase 1, além de um começo parcial da Fase 2 (Identity). Ele ainda **não é um MVP funcional**.

Já existe preparação para:

- .NET 10 fixado por `global.json`;
- Central Package Management;
- API ASP.NET Core;
- Problem Details e OpenAPI;
- rate limiting e output cache;
- autenticação JWT e autorização por política;
- SignalR;
- Entity Framework Core com Npgsql;
- ASP.NET Core Identity com IDs `Guid`;
- migration inicial de Identity;
- health checks e OpenTelemetry;
- Aspire com API, Admin e PostgreSQL;
- aplicativo MAUI Android e inclusão condicional de iOS no macOS;
- painel Blazor inicial;
- testes básicos de domínio, arquitetura, API, aplicação e ViewModel.

## Atualização de continuidade — notificações da Fase 4

Em 14 de julho de 2026, o usuário informou a conclusão de um incremento da Fase 4 com notificações internas de match:

- notificações persistentes são criadas atomicamente para os dois tutores quando surge um match;
- existem endpoints autenticados para listar notificações e marcá-las como lidas;
- o ownership é aplicado no servidor;
- a transição de leitura é monotônica;
- o mobile possui uma tela de notificações acessível pela Home;
- foi gerada a migration `Phase4MatchNotifications`;
- a documentação da Fase 4 foi atualizada.

Validações reportadas para esse incremento:

- formatador aprovado;
- projetos afetados compilados;
- 49 testes relacionados aprovados, sendo 17 de domínio, 19 de integração da API e 13 mobile;
- nenhuma falha;
- permanecem somente avisos `NU1900`, pois a consulta de vulnerabilidades do NuGet estava indisponível.

Esses resultados foram reportados pelo usuário e ainda não foram reexecutados nesta sessão. A migration foi gerada, mas não aplicada automaticamente ao banco. Uma instância antiga de `PetMach.Api` permanece aberta por outro nível de permissão e precisa ser reiniciada para carregar o incremento.

## Estado atual informado em 15 de julho de 2026

### Fase 4 — correções, descoberta mobile e notificações

- cadastro concluído redireciona para o login e perfil salvo retorna para `//home`, ambos com testes de regressão;
- descoberta mobile possui filtros por sexo, porte, raça, energia, objetivo, castração e vacinação;
- filtros são enviados à API por contrato tipado;
- existem limpeza de filtros e paginação incremental;
- o progresso está documentado em `docs/phase-4-progress.md`;
- notificações internas de match permanecem conforme o incremento documentado acima.

Validações reportadas para o incremento de filtros e redirecionamentos:

- `dotnet tool restore`, `dotnet restore` e `dotnet format --verify-no-changes` aprovados;
- 56 testes aprovados, sem reprovações;
- os projetos mobile e os novos testes compilaram antes dos bloqueios externos;
- o build completo foi bloqueado por uma instância de `PetMach.Api.exe` mantendo DLLs abertas;
- o build Android encontrou restrição de execução de `llc.exe` naquele ambiente.

### Fase 5 — chat persistente e SignalR no MAUI

Primeiro incremento:

- uma conversa única é criada atomicamente com cada match;
- a migration `Phase5Chat` inclui conversas para matches existentes;
- mensagens persistentes são limitadas a 2.000 caracteres;
- histórico paginado possui ordenação determinística;
- somente participantes de match ativo podem acessar a conversa;
- bloqueio ou unmatch interrompem o acesso imediatamente;
- `MessageReceived` é publicado pelo SignalR somente após a persistência;
- o mobile possui telas de conversas, histórico e envio;
- a política provisória de retenção está registrada em `docs/open-questions.md`;
- o progresso está documentado em `docs/phase-5-progress.md`.

Validação reportada para o primeiro incremento: formatação, backend e mobile aprovados, com 55 testes aprovados e nenhuma reprovação.

Segundo incremento:

- cliente SignalR autenticado no MAUI com token obtido pela sessão segura, sem persistência adicional;
- reconexão automática progressiva e reentrada no grupo autorizado;
- conexão encerrada ao sair da tela de chat;
- deduplicação entre histórico REST, resposta de envio e evento SignalR;
- atualização das mensagens no contexto da interface;
- teste de regressão para mensagens duplicadas.

Validação reportada para o segundo incremento: build Android e formatação aprovados, com 15 testes mobile aprovados e nenhuma falha. Permaneceu apenas o aviso `NU1900` causado pela indisponibilidade da consulta externa de vulnerabilidades do NuGet.
Terceiro incremento — estado de leitura:

- marcador individual por conversa e participante;
- atualização monotônica, sem retroceder para mensagens antigas;
- contagem de mensagens não lidas na lista de conversas;
- leitura atualizada ao abrir o histórico ou receber mensagem;
- evento SignalR `ConversationRead` publicado após a persistência;
- validação de participante, match ativo e bloqueios;
- migration `Phase5ConversationReadState`;
- testes de monotonicidade e rejeição de acesso anônimo;
- documentação da Fase 5 atualizada.

Validação reportada para o terceiro incremento: formatação aprovada, backend e mobile compilados, com 59 testes aprovados e nenhuma reprovação. Permaneceram somente avisos externos `NU1900` do NuGet.
Quarto incremento — propostas de encontro:

- propostas vinculadas a matches ativos;
- data e hora futuras, local público e observação opcional;
- estados proposta, aceita, recusada e cancelada, com transições explícitas;
- somente o destinatário pode aceitar ou recusar;
- ambos os participantes podem cancelar propostas pendentes ou aceitas;
- bloqueio e unmatch impedem transições;
- nenhuma coordenada é armazenada ou exposta;
- tela mobile para criar e responder propostas;
- migration `Phase5Meetings`;
- documentação da Fase 5 atualizada.

Validação reportada para o quarto incremento: formatação aprovada, backend e mobile compilados, com 65 testes aprovados e nenhuma reprovação. Permaneceram somente avisos externos `NU1900` do NuGet.
Quinto incremento — notificações internas de encontros:

- nova proposta notifica o outro participante;
- aceite, recusa e cancelamento notificam somente o participante oposto à ação;
- cada notificação é persistida na mesma operação da criação ou mudança de estado;
- a caixa interna passou a expor o tipo da notificação, referência opcional ao match e referência opcional ao encontro;
- foi preservada a compatibilidade com notificações de match existentes;
- índices impedem a duplicação do mesmo evento para o mesmo destinatário;
- migration `Phase5MeetingNotifications`;
- documentação da Fase 5 atualizada.

Validação reportada para o quinto incremento: formatação aprovada, backend e mobile compilados, com 66 testes aprovados e nenhuma reprovação. Permaneceram somente avisos externos `NU1900` do NuGet.
Revisão de encerramento funcional da Fase 5:

- adicionados testes mobile de paginação do histórico;
- adicionado teste de reconexão e estado visual do SignalR;
- adicionados testes de validação e transição de encontros;
- endpoints sociais documentados em `docs/api/social.md`;
- matriz de critérios e pendências registrada em `docs/phase-5-progress.md`;
- README e plano de execução atualizados.

Validação reportada para a revisão: formatação aprovada, com 70 testes aprovados e nenhuma reprovação. Permaneceram somente avisos externos `NU1900` do NuGet.

O escopo funcional da Fase 5 está implementado, mas a fase permanece formalmente em validação. Ainda é necessário executar testes autenticados de persistência e concorrência sobre PostgreSQL real via Testcontainers e aplicar as migrations em uma base descartável. Esses gates dependem da disponibilidade do Docker e impedem declarar a Fase 5 concluída pelo Definition of Done.
### Fase 6 — parceiros e espaços

Primeiro incremento:

- estabelecimento parceiro vinculado a um representante proprietário;
- registro empresarial único, disponível somente no contrato de gestão;
- permitido um estabelecimento por representante;
- espaços possuem descrição, capacidade e valor meramente informativo;
- gestão protegida por `PartnerAccess` e ownership no servidor;
- catálogo autenticado sem dados empresariais internos, com filtros por cidade e estado;
- tela mobile para explorar espaços;
- nenhuma coordenada é armazenada ou exposta e nenhum pagamento real é processado;
- migration `Phase6PartnersAndSpaces`;
- progresso registrado em `docs/phase-6-progress.md`.

Defaults provisórios documentados para a Fase 6:

- reservas exigem confirmação manual do parceiro;
- disponibilidade começa com janelas explícitas, sem recorrência, feriados ou regras de antecedência definidas;
- cancelamento não possui penalidade financeira no MVP inicial, mas registra ator e instante;
- valores são informativos e o pagamento ocorre presencialmente;
- o estabelecimento usa identificador IANA de fuso e eventos relevantes são persistidos em UTC.

Validação reportada para o primeiro incremento da Fase 6: formatação aprovada, backend e mobile compilados, com 76 testes aprovados e nenhuma reprovação. Permaneceram somente avisos externos `NU1900` do NuGet.
Segundo incremento — disponibilidade dos espaços:

- criação de janelas explícitas pelo proprietário, protegida por `PartnerAccess` e ownership;
- períodos obrigatoriamente futuros, em UTC e com duração máxima de sete dias;
- sobreposições rejeitadas com HTTP 409;
- consulta autenticada dos próximos 30 dias por padrão, limitada a intervalos de 90 dias;
- mobile permite selecionar um espaço e consultar seus horários;
- migration `Phase6SpaceAvailability`;
- testes de domínio e autorização e documentação da Fase 6 atualizados.

Validação reportada para o segundo incremento: formatação aprovada, solução compilada sem erros e 91 testes aprovados. Permaneceram somente avisos `NU1900`, causados pela indisponibilidade da auditoria online do NuGet. Testes reais de concorrência com PostgreSQL continuam pendentes porque o Docker não está disponível.

Atenção de continuidade: `docs/phase-6-progress.md` já registra também os incrementos 3–5 — reservas pendentes, cancelamento/atendimento presencial e reservas no mobile. Antes de implementar reservas novamente, confirmar o código e as validações desses incrementos e seguir o estado mais avançado do relatório.
Terceiro incremento — reservas pendentes e confirmação manual:

- tutor solicita reserva usando um cão próprio;
- toda reserva inicia como `Pending`;
- parceiro visualiza somente reservas dos próprios espaços;
- apenas o parceiro proprietário pode confirmar;
- constraint PostgreSQL impede duas reservas ativas na mesma disponibilidade;
- ownership e autorização aplicados nos endpoints;
- reservas e seu ciclo operacional foram consolidados na migration `Phase6Reservations`;
- documentação e testes atualizados.

Validação reportada para o terceiro incremento: formatação e build completo aprovados, com 96 testes aprovados e nenhum erro. Permaneceram somente avisos `NU1900` por indisponibilidade da auditoria online do NuGet.
Quarto incremento — ciclo operacional das reservas:

- cancelamento permitido ao tutor ou parceiro proprietário, sem penalidade;
- responsável e instante UTC do cancelamento são registrados;
- histórico imutável preserva todas as transições;
- conclusão do atendimento permitida somente após o início da reserva;
- ausência permitida somente após o término;
- pagamento permanece exclusivamente presencial e informativo, nos estados `AwaitingOnSite` e `RecordedOnSite`;
- endpoints de histórico protegidos por ownership;
- migration consolidada `Phase6Reservations`;
- documentação e testes de domínio e autorização atualizados.

Validação reportada para o quarto incremento: formatação e build completo aprovados, com 102 testes aprovados e nenhum erro. Permaneceu somente o aviso `NU1900` devido à indisponibilidade da auditoria online do NuGet.

O próximo passo informado pelo usuário é o mobile de reservas. Esse fluxo já aparece como incremento 5 em `docs/phase-6-progress.md`; confirmar sua implementação e validação antes de refazê-lo.

O próximo incremento informado pelo usuário é cancelamento, histórico da reserva e registro informativo de atendimento/pagamento presencial. Contudo, essas funcionalidades já aparecem descritas como incremento 4 em `docs/phase-6-progress.md`; confirmar sua implementação e validação antes de refazê-las.

As migrations `Phase4MatchNotifications`, `Phase5Chat`, `Phase5ConversationReadState`, `Phase5Meetings`, `Phase5MeetingNotifications`, `Phase6PartnersAndSpaces`, `Phase6SpaceAvailability` e a migration consolidada `Phase6Reservations` foram geradas, mas não aplicadas automaticamente ao banco. Os resultados desta seção foram informados pelo usuário e ainda não foram reexecutados nesta sessão.

## Estado histórico da análise inicial

A análise inicial de 14 de julho registrava apenas a fundação técnica e o começo parcial de Identity. Esse retrato foi superado pelos incrementos documentados das Fases 4 e 5. Para planejar ou avaliar funcionalidades, consultar prioritariamente `docs/phase-4-progress.md`, `docs/phase-5-progress.md`, `docs/phase-6-progress.md`, `docs/open-questions.md` e o código atual.

## Pontos de atenção da análise inicial

1. Diversos arquivos apresentam caracteres corrompidos, como `socializaÃ§Ã£o`. O problema parece ser texto UTF-8 anteriormente interpretado ou gravado com encoding incorreto e também afeta strings do mobile.
2. O prompt original usa o nome `DogMatch`, enquanto o repositório usa `PetMach`. Confirmar se a troca foi intencional e se a grafia `Mach` deve permanecer.
3. O Git pode emitir `fatal: unsafe repository` porque a pasta foi criada por outro usuário técnico. O README contém uma correção restrita à pasta; não usar `safe.directory=*`.
4. A documentação da Fase 1 descreve vários componentes como disponíveis, mas muitos estão apenas preparados, ainda sem fluxo funcional.
5. `AddJwtBearer()` está registrado sem parâmetros explícitos de validação; isso precisa ser concluído na implementação de Identity.
6. Testcontainers está referenciado, mas ainda não há cobertura relevante dos fluxos obrigatórios com PostgreSQL real.
7. Docker/PostgreSQL em container e iOS não haviam sido validados, segundo o relatório existente.

## Validação e limitações atuais

As validações mais recentes estão registradas nas atualizações acima e nos relatórios de fase. Não declarar uma validação completa do repositório enquanto o build integral não for reexecutado sem bloqueios externos.

Limitações conhecidas:

- instâncias antigas de `PetMach.Api.exe` podem manter DLLs abertas e devem ser encerradas antes do build e da execução do incremento novo;
- as migrations novas não foram aplicadas automaticamente ao banco;
- a execução de `llc.exe` já foi bloqueada pelo ambiente, embora o build Android posterior do incremento SignalR tenha sido reportado como aprovado;
- avisos `NU1900` decorrem da indisponibilidade da consulta externa de vulnerabilidades do NuGet;
- testes PostgreSQL via Testcontainers dependem da disponibilidade do Docker.

## Atualização de continuidade — encerramento da Fase 6 e início da Fase 7

Em 16 de julho de 2026, a Fase 6 foi formalmente concluída:

- o painel operacional do parceiro no mobile permite consultar o próprio estabelecimento e espaços, criar disponibilidades no horário local do estabelecimento com conversão para UTC pelo fuso IANA, listar reservas recebidas e confirmar, cancelar, concluir ou registrar ausência;
- todas as migrations foram aplicadas do zero em PostgreSQL real;
- foi identificado e corrigido um erro real na migration consolidada de reservas, cujas foreign keys apontavam incorretamente para um schema `Id`;
- testes concorrentes reais confirmaram que apenas uma reserva ativa pode ocupar uma disponibilidade;
- disputas concorrentes são protegidas por constraint e a operação perdedora retorna PostgreSQL `23505`;
- o arquivo `.env` está protegido pelo `.gitignore`.

O primeiro e o segundo incrementos da Fase 7 também foram entregues:

- publicações de adoção permanecem totalmente separadas de likes e matches;
- somente o tutor pode publicar um cão próprio e ativo, mediante aceite explícito de termo versionado;
- existe uma publicação por cão, garantida por constraint PostgreSQL;
- o catálogo autenticado é limitado a 100 itens, respeita bloqueios nos dois sentidos e a configuração de privacidade da região;
- candidaturas possuem os estados `Submitted`, `UnderReview`, `Approved`, `Rejected` e `Withdrawn`;
- candidato informa motivação, experiência e contexto do lar;
- ownership e bloqueios são aplicados no servidor;
- todas as transições geram histórico auditável;
- a aprovação move a publicação para `InProgress`, sem rejeitar ou aprovar automaticamente as demais candidaturas;
- existe uma candidatura por pessoa/publicação e no máximo uma candidatura aprovada por publicação;
- a constraint de aprovação única foi validada com transações concorrentes em PostgreSQL real, retornando `23505` para a disputa perdedora;
- migration `Phase7AdoptionProfiles` e migration das candidaturas foram materializadas e aplicadas.

Terceiro incremento da Fase 7 — denúncias e fila de moderação:

- denúncias podem apontar para usuário, cão, publicação de adoção ou mensagem;
- motivos são controlados por allowlist e não é permitido denunciar conteúdo próprio;
- constraint parcial única impede denúncia ativa duplicada para o mesmo denunciante e alvo;
- evidências aceitam somente JPEG, PNG ou PDF cujo tipo real seja validado;
- cada arquivo possui limite de 5 MB e cada denúncia aceita no máximo cinco evidências;
- arquivos recebem nomes gerados, ficam em armazenamento não público e o download é exclusivo da moderação;
- a fila é protegida por `AdministrationAccess` e permite transições para revisão e arquivamento;
- migration `Phase7ModerationReports` aplicada no PostgreSQL real;
- documentação e testes de autorização, armazenamento e concorrência foram atualizados.

Quarto incremento da Fase 7 — ações administrativas auditadas:

- moderação pode suspender usuário, cão ou publicação de adoção;
- a ação precisa ser compatível com o tipo do alvo e exige denúncia previamente em revisão;
- suspensão de usuário revoga sessões e reutiliza a auditoria de identidade;
- constraint garante uma única ação administrativa por denúncia;
- auditoria registra moderador, ação, alvo e instante UTC, sem justificativa ou evidência sensível;
- após a execução bem-sucedida, a denúncia passa para `Actioned`;
- migration `Phase7ModerationActions` aplicada no PostgreSQL real.

Quinto incremento da Fase 7 — adoção e denúncias no mobile:

- catálogo de publicações de adoção e publicação de cão próprio;
- aceite explícito do termo de adoção;
- candidatura com motivação, experiência e contexto do lar;
- acompanhamento e retirada de candidatura;
- suspensão da própria publicação;
- denúncia de publicação com motivo controlado;
- nova tela acessível pela home;
- regras visuais derivadas exclusivamente dos estados retornados pelo servidor.

Validação reportada para o quinto incremento: formatação, build integral e Android aprovados, com 136 testes aprovados, incluindo PostgreSQL real. Nenhum erro; permaneceram somente avisos `NU1900`.

Sexto incremento da Fase 7 — dashboard administrativo de moderação:

- login administrativo conectado à API e restrito aos papéis `Administrator` ou `Moderator`;
- sessão em cookie HTTP-only, `SameSite=Strict`, com expiração alinhada ao access token;
- JWT armazenado no cookie criptografado, sem exposição ao JavaScript;
- fila apresenta descrição, estado e evidências protegidas;
- moderação pode iniciar revisão, arquivar e aplicar suspensão compatível com usuário, cão ou publicação;
- todas as mutações são protegidas por antiforgery;
- evidências são acessadas por proxy autenticado, sem exposição da storage key.

Validação mais recente reportada:

- formatação aprovada;
- build integral aprovado;
- 137 testes aprovados, incluindo PostgreSQL real;
- nenhum erro;
- permanecem somente avisos `NU1900` porque a auditoria NuGet está offline.

Sétimo incremento e encerramento funcional da Fase 7 — evidências mobile e revisão de segurança:

- seleção opcional de evidência por seletor específico no mobile;
- upload de JPEG, PNG ou PDF;
- limite real de 5 MB, com margem separada para o overhead multipart HTTP;
- tipo detectado pela assinatura real do arquivo, sem confiar no tipo declarado pelo dispositivo;
- evidência vinculada automaticamente à denúncia criada;
- revisão final de segurança documentada.

Validação de encerramento reportada:

- formatação aprovada;
- build integral e Android aprovados;
- 137 testes aprovados, incluindo PostgreSQL real;
- nenhum erro;
- permanecem somente avisos `NU1900` porque a auditoria NuGet está offline.

A Fase 7 está funcionalmente concluída. Permanecem como bloqueadores explícitos para produção:

- política jurídica de retenção e eliminação de denúncias e evidências;
- object storage protegido, criptografado, com backup e auditoria de acesso;
- matriz definitiva de permissões administrativas;
- key ring persistente e protegido para os cookies do Admin.

## Próxima etapa recomendada

Iniciar a Fase 8 com consolidação da experiência mobile, acessibilidade e estados consistentes de loading, vazio, erro e sucesso. Preservar os bloqueadores de produção da Fase 7 como pendências explícitas, sem tratá-los como concluídos.

## Regras para retomada

- Não alterar arquivos antes de ler as instruções do repositório e verificar mudanças existentes.
- Não expor senhas, tokens, localização exata ou dados sensíveis de saúde.
- Não criar repositório genérico ou Unit of Work redundante sobre `DbContext`.
- Não misturar adoção com likes.
- Não implementar pagamento real ou reprodução no MVP.
- Não remover requisitos para simplificar a entrega.
- Não deixar TODO sem registrar no backlog.
- Após cada incremento, executar os comandos de qualidade definidos no `AGENTS.md` e registrar qualquer limitação real.

