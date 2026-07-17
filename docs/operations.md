# Operação e execução

Todos os comandos partem da raiz do repositório e usam PowerShell.

## Pré-requisitos

- .NET SDK compatível com `global.json`;
- Docker Desktop para Compose, Aspire com PostgreSQL e testes Testcontainers;
- workload `maui-android` e Android SDK 36 para o aplicativo;
- emulador ou dispositivo Android para executar o Mobile;
- macOS com toolchain Apple para compilar e assinar iOS.

O projeto aceita o `dotnet` instalado no sistema. Nesta máquina também pode
existir uma instalação local em `.dotnet`:

```powershell
$dotnet = if (Test-Path .\.dotnet\dotnet.exe) {
    '.\.dotnet\dotnet.exe'
} else {
    'dotnet'
}
```

## Preparação inicial

```powershell
& $dotnet tool restore
& $dotnet restore PetMach.slnx
```

## Execução local: PostgreSQL, API e Admin

Inicie apenas o PostgreSQL:

```powershell
$env:PETMACH_POSTGRES_PASSWORD = 'uma-senha-local-forte'
docker compose up -d postgres
```

Inicialize User Secrets uma vez. Se já estiver inicializado, o comando apenas
informa essa condição:

```powershell
& $dotnet user-secrets init `
  --project backend/src/PetMach.Api/PetMach.Api.csproj
```

Configure a mesma senha na connection string da API sem versioná-la:

```powershell
$connectionString = "Host=localhost;Port=5432;Database=petmach;Username=petmach;Password=$env:PETMACH_POSTGRES_PASSWORD"
& $dotnet user-secrets set `
  'ConnectionStrings:petmach' `
  $connectionString `
  --project backend/src/PetMach.Api/PetMach.Api.csproj
```

Aplique as migrations:

```powershell
& $dotnet ef database update `
  --project backend/src/PetMach.Infrastructure/PetMach.Infrastructure.csproj `
  --startup-project backend/src/PetMach.Api/PetMach.Api.csproj
```

### Terminal 1 — API

```powershell
& $dotnet run `
  --project backend/src/PetMach.Api/PetMach.Api.csproj `
  --launch-profile http
```

A API fica em `http://localhost:5049`.

### Terminal 2 — Admin

```powershell
& $dotnet run `
  --project backend/src/PetMach.Admin/PetMach.Admin.csproj `
  --launch-profile http
```

O Admin fica em `http://localhost:5175` e usa a API em
`http://localhost:5049/`.

### Verificação local

```powershell
Invoke-WebRequest http://localhost:5049/health/live
Invoke-WebRequest http://localhost:5049/health/ready
Invoke-WebRequest http://localhost:5175/health/ready
```

`/health/live` verifica o processo. A readiness da API também verifica a
conexão com PostgreSQL.

## Execução completa com Docker Compose

Crie um `.env` local a partir de `.env.example` e substitua os placeholders.
O arquivo `.env` não deve ser versionado.

```dotenv
PETMACH_POSTGRES_PASSWORD=uma-senha-local-forte
Identity__SigningKey=uma-chave-local-com-pelo-menos-32-caracteres
```

Valide e inicie:

```powershell
docker compose config
docker compose build
docker compose up -d --wait --wait-timeout 180
docker compose ps -a
```

Endpoints publicados:

| Serviço | URL no host | URL entre containers |
|---|---|---|
| PostgreSQL | `localhost:5432` | `postgres:5432` |
| API | `http://localhost:5080` | `http://api:8080` |
| Admin | `http://localhost:5081` | `http://admin:8080` |

O serviço `migrator` deve aparecer como concluído com exit code `0`. Ele
aguarda PostgreSQL saudável, aplica as migrations reais e termina. A API só
inicia depois disso; o Admin só inicia depois da API saudável.

Verifique:

```powershell
Invoke-WebRequest http://localhost:5080/health/live
Invoke-WebRequest http://localhost:5080/health/ready
Invoke-WebRequest http://localhost:5081/health/ready
docker compose logs migrator
```

Para encerrar e remover dados locais da composição:

```powershell
docker compose down -v --remove-orphans
```

O uso de `-v` remove o volume PostgreSQL. Não o use se quiser preservar os
dados do ambiente local.

## Execução com Aspire

Com Docker Desktop disponível:

```powershell
& $dotnet run `
  --project backend/src/PetMach.AppHost/PetMach.AppHost.csproj
```

O AppHost declara PostgreSQL, banco `petmach`, API e Admin. A API recebe a
referência do banco; o Admin recebe a referência da API e usa service discovery.
O dashboard Aspire mostra recursos, endpoints, logs, traces e health checks.

O Compose possui um migrator dedicado. No fluxo Aspire, aplique migrations pelo
comando EF explícito antes de depender de um schema novo.

## Execução do Mobile Android

Mantenha API e PostgreSQL ativos pelo fluxo local. O padrão Debug do aplicativo
aponta para:

```text
http://10.0.2.2:5049/
```

No emulador Android, `10.0.2.2` representa o computador host. Para dispositivo
físico ou outra topologia, configure uma URL absoluta:

```powershell
$env:PETMACH_API_BASE_URL = 'http://192.168.1.10:5049/'
```

`PETMACH_API_BASE_URL` contém somente um endpoint, não credenciais. O aparelho
deve alcançar essa URL, e firewall/rede podem precisar permitir a porta.

Build:

```powershell
& $dotnet build `
  frontend/src/PetMach.Mobile/PetMach.Mobile.csproj `
  -f net10.0-android
```

Execução em emulador ou dispositivo conectado:

```powershell
& $dotnet build `
  frontend/src/PetMach.Mobile/PetMach.Mobile.csproj `
  -f net10.0-android `
  -t:Run
```

Internet não é necessária para a comunicação entre emulador e API local depois
que SDKs, workloads, pacotes e imagens já estiverem disponíveis. Restore de
pacotes, download de imagens Docker e integrações externas exigem internet.

## Solução de problemas

### API inicia, mas readiness falha

- confirme `docker compose ps`;
- confira a senha usada pelo PostgreSQL e pela connection string;
- execute ou confira o migrator;
- leia `docker compose logs postgres migrator api`.

### Admin abre, mas não acessa a API

- localmente, confirme API em `http://localhost:5049/`;
- no Compose, confirme `PetMachApi__BaseUrl=http://api:8080/`;
- não use `localhost` para comunicação entre containers.

### Mobile informa que não alcança a API

- confirme `/health/ready` no host;
- no emulador Android, use `10.0.2.2`, não `localhost`;
- em dispositivo físico, use o IP alcançável do computador;
- confirme firewall e que API/dispositivo estão na rede adequada.

### Testcontainers falha ao iniciar

- inicie o Docker Desktop;
- execute `docker version` e confirme a seção `Server`;
- não defina uma variável para fazer o teste “passar”: a fixture cria o banco
  automaticamente e falha de forma visível sem Docker.
