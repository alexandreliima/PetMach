---
title: Documentation Taxonomy
category: Engineering
type: Reference
status: Canonical
version: 1.0
owner: PetMatch Engineering
last_reviewed: 2026-07-19
related_documents:
  - ../README.md
  - Documentation-Governance.md
  - Documentation-Lifecycle.md
---

# Documentation Taxonomy

## Categories

| Category | Directory | Content |
|---|---|---|
| Product | `00-product/` | Vision, scope, personas, glossary and roadmap |
| Engineering | `01-engineering/` | Governance, contribution, conventions and security |
| Features | `02-features/` | User and system behavior by feature |
| Architecture | `03-architecture/` | System structure, domain, diagrams and decisions |
| Design | `04-design/` | Design System, UX, accessibility and motion |
| API | `05-api/` | HTTP contracts, authentication and integration references |
| Testing | `06-testing/` | Test strategy, categories, coverage and fixtures |
| Operations | `07-operations/` | Local execution, containers, runbooks and deployment |
| History | `99-history/` | Superseded reports, guides and preserved records |

## Document types

| Type | Purpose |
|---|---|
| Feature | Describes user value, behavior, states and maturity |
| ADR | Records an approved architectural decision and consequences |
| RFC | Proposes a substantial change for review before implementation |
| Guide | Teaches a task or workflow |
| Reference | Provides structured facts intended for consultation |
| Runbook | Provides safe operational diagnosis and recovery steps |
| Checklist | Defines a repeatable verification sequence |
| Specification | Defines normative requirements or architecture |

## Placement rules

- Choose the category by the document's primary audience and purpose.
- Use links instead of duplicating content across categories.
- ADRs remain in their current directory until a dedicated migration is approved.
- Existing documents retain their paths during the governance bootstrap.
- Historical placement does not mean deletion; it means the content is preserved
  without being treated as current authority.

## Naming

- New filenames use descriptive English names with `.md`.
- Numbered prefixes are reserved for category ordering and formally numbered ADRs
  or RFCs.
- The product is presented as PetMatch; technical identifiers remain `PetMach`
  until a separate technical change is approved.

