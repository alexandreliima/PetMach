---
title: Documentation Lifecycle
category: Engineering
type: Specification
status: Canonical
version: 1.0
owner: PetMatch Engineering
last_reviewed: 2026-07-19
related_documents:
  - Documentation-Governance.md
  - Documentation-Taxonomy.md
---

# Documentation Lifecycle

## Official statuses

### Canonical

The official, reviewed and currently applicable explanation of a subject.
There should be only one canonical document for the same scope.

### Working Draft

Content under discussion or construction. It may not be used as evidence that a
feature, contract or operational capability exists.

### Historical

A preserved record of a past phase, decision context or implementation state.
It is not maintained as a description of the current system.

### Deprecated

Content intentionally replaced or no longer recommended. It must identify its
canonical successor and may be removed only through an explicit documentation
cleanup.

## Transitions

```text
Working Draft -> Canonical
Canonical -> Deprecated
Canonical -> Historical
Deprecated -> Historical
```

A document may return from `Working Draft` to further drafting without becoming
canonical. A historical document must not silently return to canonical status;
it requires a new review against the repository.

## Review process

1. Select the correct template and category.
2. Complete metadata and distinguish current state from plans.
3. Verify claims against code, contracts, migrations, tests or approved ADRs.
4. Validate links and sensitive-data handling.
5. Obtain review from the responsible area.
6. Mark as `Canonical` only after approval.
7. Reassess when the related implementation changes.

## Versioning

- `0.x` identifies a working draft.
- `1.0` identifies the first canonical version.
- Increment the minor version for compatible clarification.
- Increment the major version when scope or normative guidance changes materially.
- Historical reports may retain their original version and date.

## Deprecation and history

A deprecated document must display:

- the reason for deprecation;
- the replacement document;
- the deprecation date;
- any temporary compatibility guidance.

Migration to `99-history/` is performed only in an explicitly scoped future
increment. This governance sprint does not move existing files.

