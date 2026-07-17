# Guia didático — Fase 3, incremento 1

> Registro histórico do primeiro incremento da Fase 3. Para o estado atual,
> consulte [Estado técnico atual](current-state.md) e
> [Guia didático do PetMach](guia-do-projeto.md).

## O que mudou

O PetMach deixou de ter apenas uma tela mobile de demonstração. Agora existe o começo da experiência real do usuário:

```text
Boas-vindas
   ├── Cadastro → API de Identity
   └── Login    → API de Identity → Home autenticada
```

Também foi criado o primeiro domínio da Fase 3: o perfil do tutor, com persistência e endpoints autenticados.

## Telas mobile

### Boas-vindas

É a porta de entrada visual do aplicativo. Permite escolher entre abrir o cadastro ou o login.

```text
WelcomeViewModel
   ├── OpenLoginCommand        → rota "login"
   └── OpenRegistrationCommand → rota "register"
```

### Cadastro

A tela coleta os dados necessários para criar a conta e chama a API de Identity. O ViewModel controla validação, carregamento, sucesso e mensagens amigáveis de falha.

O cadastro ainda exige confirmação de e-mail antes que a conta possa fazer login.

### Login

A tela envia e-mail e senha para a API. Em caso de sucesso:

```text
LoginPage
   ↓ utiliza
LoginViewModel
   ↓ chama
IAuthApiClient
   ↓ POST /api/v1/auth/login
API
   ↓ devolve tokens
AuthenticationSession
   ↓
SecureTokenStore
   ↓ guarda no SecureStorage
ShellNavigator
   ↓
//home
```

### Home autenticada

É a primeira tela exibida depois de um login bem-sucedido. Ainda é uma home inicial; os módulos de cães, descoberta, matches e reservas serão adicionados futuramente.

## Shell Navigation

O `AppShell` organiza as páginas e rotas do aplicativo. Atualmente registra as rotas de login e cadastro e possui destinos principais, como boas-vindas e home.

O `ShellNavigator` implementa uma interface de navegação usada pelos ViewModels. Isso evita colocar chamadas diretas à interface dentro da lógica testável.

```text
ViewModel
   ↓ solicita uma rota
IMobileNavigator
   ↓ implementado por
ShellNavigator
   ↓ chama
Shell.Current.GoToAsync(...)
```

Essa separação permite testar se o ViewModel pediu a navegação correta sem abrir um emulador.

## Conexão do Android com a API

No emulador Android, `localhost` aponta para o próprio emulador. Para chegar à API executada no computador, o aplicativo utiliza:

```text
http://10.0.2.2:5049
```

O uso de HTTP sem TLS foi permitido somente no build Debug Android. Produção deverá usar HTTPS.

## Perfil do tutor

Foi criado o agregado `TutorProfile`, responsável pelos dados pessoais e preferências do tutor:

- nome;
- sobrenome, conforme definido pelo contrato atual;
- telefone;
- cidade;
- estado;
- biografia;
- preferências de privacidade.

Os arquivos principais são:

- `backend/src/PetMach.Domain/Tutors/TutorProfile.cs` — entidade e regras;
- `backend/src/PetMach.Application/Tutors/ITutorProfileService.cs` — operações necessárias;
- `backend/src/PetMach.Application/Tutors/TutorProfileValidator.cs` — validação de entrada;
- `backend/src/PetMach.Infrastructure/Tutors/TutorProfileService.cs` — leitura e gravação;
- `backend/src/PetMach.Contracts/Tutors/TutorContracts.cs` — formatos da API;
- `backend/src/PetMach.Api/Controllers/TutorsController.cs` — endpoints HTTP.

## Endpoints do tutor

### Consultar o próprio perfil

```http
GET /api/v1/tutors/me
Authorization: Bearer {access-token}
```

### Criar ou atualizar o próprio perfil

```http
PUT /api/v1/tutors/me
Authorization: Bearer {access-token}
Content-Type: application/json
```

Ambos exigem a política `TutorAccess`.

## Como o ownership funciona

Ownership significa determinar quem é o dono de um registro. O perfil pertence ao usuário autenticado.

```text
Access token
   ↓ contém o ID do usuário
API extrai o userId autenticado
   ↓
TutorProfileService lê ou altera somente esse perfil
```

O aplicativo não envia um `ownerId` para escolher o proprietário. Isso impede que um usuário tente alterar o perfil de outra pessoa mudando um ID na requisição.

## Persistência

A migration `Phase3TutorProfile` adiciona ao PostgreSQL a estrutura necessária para guardar os perfis dos tutores.

O relatório informa que o modelo EF Core ficou consistente e sem mudanças pendentes após a migration. A aplicação e validação em PostgreSQL real continuam dependendo de um ambiente de banco disponível.

## O que já pode ser visto

Com a API e o PostgreSQL ativos e um emulador ou aparelho Android conectado, já é possível visualizar:

1. tela de boas-vindas;
2. tela de cadastro;
3. tela de login;
4. mensagens de validação e carregamento;
5. home inicial depois da autenticação.

Para que cadastro e login funcionem, a API deve estar acessível na porta `5049`, o banco deve conter as migrations e o aparelho/emulador deve conseguir alcançar o computador.

## O que ainda não pode ser visto

O backend do perfil do tutor possui `GET` e `PUT`, mas o formulário visual de edição do perfil ainda não está conectado. Esse é o próximo incremento planejado.

Também ainda não estão implementados:

- cadastro e gestão de cães;
- catálogo de raças;
- galeria e upload protegido;
- saúde, vacinas e vermifugação;
- descoberta, likes e matches;
- chat funcional;
- parceiros e reservas.

## Testes adicionados

O incremento adicionou testes para:

- regras do `TutorProfile`;
- validação do perfil;
- autorização dos endpoints;
- comandos e navegação mobile.

O relatório `docs/phase-3-progress.md` registra:

- build dos 15 projetos;
- build Android aprovado;
- zero warnings e zero erros;
- 28 testes aprovados;
- formatação aprovada;
- migration consistente.

Esses resultados foram conferidos no relatório e a presença do código foi inspecionada. Eles não foram executados novamente durante a criação deste guia.

## Estado atualizado

| Área | Estado atual |
|---|---|
| Fase 2 — Identity | Implementada |
| Boas-vindas, cadastro e login mobile | Implementados |
| Home mobile | Versão inicial implementada |
| Navegação Shell/MVVM | Implementada para o fluxo atual |
| Sessão e SecureStorage | Implementados |
| Perfil do tutor no backend | Entidade, GET, PUT e migration implementados |
| Formulário mobile do tutor | Próximo incremento |
| Cães, fotos e saúde | Não implementados |
| Fase 3 completa | Em andamento |

## Próximos incrementos

1. Conectar o formulário mobile do tutor à API.
2. Implementar cães, catálogo de raças, preferências e ownership.
3. Implementar galeria e uploads protegidos.
4. Implementar vacinas, vermifugação, comprovantes e indicador público derivado.
5. Validar persistência com PostgreSQL/Testcontainers quando Docker estiver disponível.
