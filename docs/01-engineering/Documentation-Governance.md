---
title: Documentation Governance
category: Engineering
type: Specification
status: Canonical
version: 1.0
owner: PetMatch Engineering
last_reviewed: 2026-07-19
related_documents:
  - ../README.md
  - Documentation-Taxonomy.md
  - Documentation-Lifecycle.md
  - ../templates/README.md
---

# Documentation Governance

## Purpose

Esta política define como a documentação do PetMatch é criada, revisada,
classificada e mantida. Ela não altera a autoridade do código, migrations,
contratos executáveis ou ADRs aprovados.

## Required metadata

Todo novo documento deve começar com o seguinte cabeçalho YAML:

```yaml
---
title: "<document title>"
category: "<document category>"
type: "<document type>"
status: "<document status>"
version: "<document version>"
owner: "<team or role>"
last_reviewed: YYYY-MM-DD
related_documents:
  - "<relative path>"
---
```

Os campos são obrigatórios. `related_documents` pode ser uma lista vazia.
Datas usam ISO 8601 e links devem ser relativos ao documento.

## Sources of truth

- Código e testes confirmam o comportamento executável.
- Contratos e OpenAPI confirmam interfaces públicas.
- Migrations confirmam o schema persistido.
- ADRs confirmam decisões arquiteturais aprovadas.
- Documentos `Canonical` explicam o estado vigente sem substituir essas fontes.
- Planos e ideias devem ser identificados como `Working Draft` ou `Planned` no texto.

## Responsibilities

O responsável indicado no campo `owner` deve:

- manter o conteúdo coerente com a implementação;
- revisar links e documentos relacionados;
- distinguir estado atual de intenção futura;
- solicitar revisão das áreas afetadas;
- atualizar a data de revisão somente após inspeção real.

## Change rules

- Mudanças técnicas relevantes devem atualizar a documentação correspondente no
  mesmo incremento.
- Não se deve copiar contratos extensos quando um link para a fonte executável é
  suficiente.
- Documentos históricos não devem ser reescritos para parecer atuais.
- Um documento substituído deve apontar para seu sucessor antes de ser marcado
  como `Deprecated`.
- Movimentações devem preservar rastreabilidade e corrigir links na mesma mudança.

## Review checklist

- Metadados completos e válidos.
- Estado documental correto.
- Afirmações confirmadas no repositório.
- Planos claramente separados do comportamento atual.
- Links relativos válidos.
- Nenhum segredo ou dado pessoal.
- Linguagem consistente e termos definidos.
- Documento sucessor indicado quando aplicável.

