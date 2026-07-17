# Estratégia e execução de testes

## Suítes

| Projeto | Cobertura principal |
|---|---|
| `PetMach.Domain.Tests` | Invariantes e transições de domínio. |
| `PetMach.Application.Tests` | Validadores e registro de dependências. |
| `PetMach.Architecture.Tests` | Direção das dependências entre camadas. |
| `PetMach.Api.IntegrationTests` | Contratos HTTP, autenticação/autorização e PostgreSQL real. |
| `PetMach.Mobile.Tests` | ViewModels, sessão, navegação, refresh e clientes HTTP. |

## Gate completo

```powershell
$dotnet = if (Test-Path .\.dotnet\dotnet.exe) {
    '.\.dotnet\dotnet.exe'
} else {
    'dotnet'
}

& $dotnet tool restore
& $dotnet restore PetMach.slnx
& $dotnet format PetMach.slnx --verify-no-changes --no-restore
& $dotnet build PetMach.slnx --no-restore
& $dotnet test PetMach.slnx --no-build --collect:'XPlat Code Coverage'
```

O atalho equivalente é:

```powershell
.\scripts\quality.ps1
```

## Persistência com PostgreSQL real

Os testes `Category=PostgreSQL` não dependem de PostgreSQL instalado manualmente
nem de `PETMACH_TEST_CONNECTION`. A fixture compartilhada usa Testcontainers
com a imagem fixada:

```text
postgres:18.0-alpine
```

Ciclo de vida:

1. gera senha efêmera em memória;
2. inicia um container PostgreSQL para a coleção xUnit;
3. cria `PetMachDbContext` com Npgsql;
4. aplica as migrations reais com `Database.MigrateAsync`;
5. limpa todas as tabelas de aplicação antes e depois de cada teste;
6. preserva `__EFMigrationsHistory`;
7. descarta o container ao final.

A coleção desabilita paralelismo interno porque os casos compartilham o banco e
o reset. Um container por coleção evita o custo e a instabilidade de um
container por teste, enquanto o truncamento com `RESTART IDENTITY CASCADE`
mantém isolamento determinístico.

Execute apenas esses testes:

```powershell
& $dotnet test `
  backend/tests/PetMach.Api.IntegrationTests/PetMach.Api.IntegrationTests.csproj `
  --filter 'Category=PostgreSQL'
```

O resultado esperado atual é **5 aprovados, 0 falhas e 0 ignorados**:

- conexão real e versão PostgreSQL 18;
- 18 migrations aplicadas do zero e nenhuma pendente;
- reset remove dados e preserva o histórico de migrations;
- concorrência rejeita reserva ativa duplicada;
- concorrência rejeita múltiplas aprovações na mesma publicação de adoção.

Os dois cenários concorrentes exigem a violação PostgreSQL `23505` e conferem o
nome da constraint. Não reduzem a validação a comportamento em memória.

## Comportamento sem Docker

Se o daemon Docker estiver indisponível, a inicialização da fixture lança uma
falha explícita com a imagem requerida e a orientação de ambiente. Os testes não
usam `return` antecipado, skip silencioso, EF Core InMemory, SQLite ou
`EnsureCreated`.

Essa decisão impede falso verde: uma execução sem persistência real não pode ser
contabilizada como aprovada.

## Testes da sessão Mobile

O núcleo Mobile possui regressões para:

- restauração de sessão e criação da raiz adequada;
- nova `AppShell` após autenticação;
- raiz pública nova em logout ou invalidação;
- refresh compartilhado entre requisições simultâneas;
- repetição única após `401`;
- ausência de repetição infinita em `401` persistente;
- limpeza segura do token store;
- parada de conexões autenticadas;
- navegação solicitada pelos ViewModels.

Como `PetMach.Mobile.Core` não depende da plataforma, esses testes executam em
`net10.0` sem emulador.

## Build Mobile

O gate da solução inclui Android no Windows porque o projeto Mobile usa
`net10.0-android`. O target iOS é adicionado condicionalmente no macOS e exige a
toolchain Apple:

```powershell
& $dotnet build `
  frontend/src/PetMach.Mobile/PetMach.Mobile.csproj `
  -f net10.0-android
```

## Regras para evidência

- não declarar Testcontainers aprovado quando Docker não iniciou;
- não substituir PostgreSQL por provider em memória ou SQLite;
- registrar o comando, exit code e contagem real da execução;
- manter migrations e constraints como fonte do schema de teste;
- preservar os testes concorrentes ao alterar reservas ou adoção.
