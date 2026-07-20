---
title: PTM-009A — Settings Shell, About and Logout
category: Features
type: Feature
status: Working Draft
version: 0.2
owner: PetMatch Mobile
last_reviewed: 2026-07-20
related_documents:
  - README.md
  - ../04-design/README.md
  - ../../frontend/README.md
---

# PTM-009A — Settings Shell, About and Logout

## Summary

Este incremento da Epic **PTM-009 — User Settings & Preferences** estabelece o
hub de Configurações do aplicativo Mobile sem alterar
backend, contratos, persistência ou sessão. O incremento entrega navegação,
informações locais do aplicativo e delegação do logout ao coordenador existente.

## Actors

- Tutor autenticado: consulta as opções e encerra a própria sessão.
- Aplicativo Mobile: apresenta metadados locais e coordena navegação.
- Serviço de sessão existente: revoga a sessão remota e remove os tokens locais.

## User flow

1. O tutor abre **Configurações** na seção **Conta e serviços**.
2. O hub apresenta as áreas disponíveis e identifica as futuras como
   **Em breve**.
3. **Sobre o PetMatch** abre a página existente no contexto autenticado e mostra
   nome, versão e build obtidos localmente.
4. **Sair da conta** solicita confirmação.
5. Ao confirmar, o fluxo existente de logout encerra conexões autenticadas,
   limpa a sessão e abre uma nova raiz pública.

## Business rules

- Itens **Em breve** não navegam.
- Configurações não cria uma nova aba no `AppShell`.
- Logout depende exclusivamente de `ILogoutCoordinator`.
- O ViewModel não impõe timeout ao logout; confirmação e política de sessão
  permanecem sob responsabilidade das camadas existentes.
- Cancelar a confirmação não altera a sessão.
- Execuções simultâneas de logout são ignoradas.
- Uma falha restaura o estado e permite nova tentativa.
- Falhas inesperadas apresentam mensagem amigável sem detalhe técnico.
- A versão exibida é a versão do aplicativo, não a versão da API.

## Interfaces and dependencies

- `SettingsPage` e `SettingsViewModel`;
- `AboutPage` e `AboutViewModel`;
- `IConfirmationService`;
- `IAppInformationProvider`;
- `IMobileNavigator`;
- `ILogoutCoordinator`;
- Design System Mobile existente.

Rotas:

- `settings`;
- `about?source=settings`.

## Security and privacy

O incremento não acessa `SecureStorage` diretamente e não manipula tokens.
Termos, política e licenças permanecem desabilitados porque não existe fonte
oficial confirmada no aplicativo.

## States and errors

- carregamento inicial;
- conteúdo normal;
- item indisponível com indicação textual;
- item indisponível permanece perceptível ao leitor de tela e não navega;
- confirmação cancelada;
- logout em processamento;
- prevenção de duplo logout;
- reset de estado e retry após falha;
- erro recuperável e não técnico.

## Test evidence

- `SettingsViewModelTests`;
- `AboutViewModelTests`;
- `HomeViewModelTests`;
- catálogo de rotas consumido pelo `AppShell`;
- testes existentes de `LogoutCoordinator`;
- testes existentes de ciclo de vida e navegação de raiz.

## Limitations

O Mobile.Core ainda não possui uma abstração de logging ou telemetria. Falhas de
navegação e do coordenador são convertidas em mensagens amigáveis, mas não são
registradas nesta Sprint para evitar a criação de infraestrutura paralela.

PTM-009A não implementa edição de conta, tema, idioma, preferências de
notificação, privacidade, alteração de senha, exclusão, sessões ativas, foto,
exportação ou push notifications.

## Planned Epic increments

Os itens abaixo pertencem à Epic PTM-009, mas permanecem planejados e não fazem
parte do comportamento implementado por PTM-009A:

- PTM-009B — aparência e tema local;
- PTM-009C — Minha Conta;
- PTM-009D — notificações e privacidade;
- PTM-009E — segurança e encerramento de conta.
