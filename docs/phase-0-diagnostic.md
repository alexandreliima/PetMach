# Diagnóstico — Fase 0

Data: 2026-07-13

## Estado encontrado

- Diretório alvo existente e vazio.
- Nenhum repositório Git inicializado no diretório.
- Nenhum `AGENTS.md`, convenção, código, solução, build ou teste preexistente.
- VS Code 1.117.0 e Git 2.36.0 disponíveis pela linha de comando.
- SDKs .NET instalados: 3.1.426, 5.0.408, 5.0.416 e 9.0.203.
- SDK .NET 10 não instalado.
- Workloads locais Android, iOS, Mac Catalyst e MAUI Windows pertencem à linha .NET 9.
- Docker não está disponível por enquanto, conforme decisão do responsável pelo produto.
- Não há Mac build host por enquanto; o build iOS não poderá ser validado localmente.
- CI remota não será configurada nesta etapa.

## Consequências

- Não é possível criar, compilar ou testar corretamente projetos `net10.0` na configuração atual.
- O `global.json` não será inventado na Fase 0; será gerado na Fase 1 com a versão .NET 10 efetivamente instalada.
- Os workloads MAUI 10 e as ferramentas de container devem ser validados antes do primeiro build completo.
- Como não havia código, não existia restore/build a executar. Esta entrega contém somente documentação e configuração do workspace, conforme o limite da Fase 0.

## Ações realizadas

- Criado `AGENTS.md` com padrões, segurança, testes e Definition of Done.
- Registrados escopo, arquitetura, estrutura, domínio, riscos, backlog e perguntas abertas.
- Registradas sete decisões arquiteturais iniciais.
- Criado workspace do VS Code com áreas separadas para backend e frontend.
- Nenhum projeto, migration ou funcionalidade de produto foi implementado.

## Gate para Fase 1

Antes de iniciar a fundação:

1. Aprovar esta análise e resolver somente perguntas classificadas como bloqueadoras.
2. Instalar SDK .NET 10 x64 e workloads MAUI 10 necessários.
3. Inicializar Git e criar o baseline documental, sem pipeline remoto por enquanto.
4. Escolher a versão instalada do SDK para `global.json` e versões compatíveis de pacotes.
5. Registrar PostgreSQL/Testcontainers/Aspire completo e build iOS como gates adiados até haver Docker e Mac disponíveis.
