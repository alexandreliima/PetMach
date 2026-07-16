# AGENTS.md — PetMach

Este arquivo rege todo o repositório. Arquivos `AGENTS.md` mais específicos podem acrescentar regras em subdiretórios, sem enfraquecer segurança, privacidade ou qualidade.

## Arquitetura

- Manter um monólito modular. Não introduzir microserviços sem ADR aprovado e evidência operacional.
- Separar fisicamente `backend/` e `frontend/`, preservando contratos claros entre eles.
- Aplicar Clean Architecture de forma pragmática, DDD nos conceitos com regras reais e Vertical Slices nos casos de uso.
- Módulos não acessam tabelas ou detalhes internos de outros módulos diretamente. Integrações ocorrem por contratos, serviços de aplicação ou eventos de domínio/integração justificados.
- Não criar repositório genérico, Unit of Work sobre `DbContext`, camada sem responsabilidade ou CQRS/MediatR universal.
- Entidades de domínio nunca são contratos HTTP.

## Código

- Usar .NET 10 LTS, C# suportado pelo SDK fixado, nullable e implicit usings.
- Tratar warnings como erros em Domain e Application.
- Usar `Guid` como estratégia única de identificadores e `DateTimeOffset` em UTC para eventos relevantes.
- Toda I/O assíncrona aceita `CancellationToken`; não usar `.Result` ou `.Wait()`.
- Preferir nomes explícitos, tipos pequenos e invariantes no domínio. Não criar classes genéricas `Utils`.
- Evitar strings mágicas, exceções silenciosas e lógica de negócio em endpoints/controllers.
- Pacotes são versionados somente em `Directory.Packages.props`.

## Segurança e LGPD

- Negar acesso por padrão e aplicar autorização por política no servidor.
- Nunca registrar senha, token, documento, localização exata ou dado sensível de saúde.
- Nunca retornar coordenadas exatas de outro tutor. Distância é calculada no servidor e arredondada.
- Tokens ficam em `SecureStorage` no mobile; segredos não entram no repositório.
- Uploads exigem limite, allowlist de tipo real, nome gerado e armazenamento protegido.
- Operações administrativas e mudanças críticas geram auditoria sem payload sensível.
- Exclusão de conta deve anonimizar ou eliminar dados conforme retenção documentada.

## Banco e consistência

- PostgreSQL é a fonte de verdade; EF Core sem abstrações redundantes.
- Mudanças de esquema exigem migration revisada.
- Reservas usam transação, constraint de não sobreposição adequada e concorrência otimista.
- Likes/matches possuem constraints para impedir auto-like e duplicidade.
- Testes de persistência usam PostgreSQL real via Testcontainers, não somente provider InMemory.

## Testes e comandos obrigatórios

Antes de concluir um incremento, executar na raiz:

```powershell
dotnet tool restore
dotnet restore PetMach.slnx
dotnet format PetMach.slnx --verify-no-changes
dotnet build PetMach.slnx --no-restore
dotnet test PetMach.slnx --no-build --collect:"XPlat Code Coverage"
```

Enquanto a solução ainda não existir (Fase 0), validar documentação, links e estrutura manualmente. Nunca declarar build/testes aprovados sem executá-los.

## Regras de testes

- Cada regra de domínio tem teste unitário de sucesso e falha relevante.
- Cada endpoint possui testes de contrato, validação, autenticação/autorização e erro.
- Fluxos de cadastro, login, refresh, cão, filtros, like/match, bloqueio, chat, reserva, adoção, denúncia e administração são obrigatórios.
- Testes devem ser determinísticos; relógio, IDs e integrações externas são controláveis.
- Um bug corrigido recebe teste de regressão.

## Documentação e decisões

- Mudanças arquiteturais exigem ADR em `docs/decisions/`.
- Ambiguidades de negócio vão para `docs/open-questions.md`; não inventar regra crítica.
- TODOs só são aceitos quando vinculados a item explícito do backlog.
- Atualizar README, OpenAPI e diagramas no mesmo incremento afetado.

## Restrições

- Não implementar reprodução/cruzamento, pagamentos reais, áudio, vídeo ou chamadas no MVP.
- Não misturar adoção com o fluxo de likes.
- Não expor dados médicos por padrão.
- Não incluir Redis, PostGIS, broker ou serviço externo sem necessidade mensurada e decisão registrada.

## Definition of Done

Uma entrega está pronta somente quando compila sem warnings, passa nos testes e no formatador, aplica validação/autorização/tratamento de erros, não expõe dados sensíveis, possui observabilidade adequada, contratos e documentação atualizados, migration consistente quando aplicável e nenhum requisito removido silenciosamente.
