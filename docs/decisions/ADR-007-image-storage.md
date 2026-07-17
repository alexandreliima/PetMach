# ADR-007 — Armazenamento protegido de imagens

- Status: Aceita
- Data: 2026-07-13

## Contexto

Perfis, cães, comprovantes de vacina, parceiros, espaços e denúncias recebem arquivos com níveis de sensibilidade distintos. O provedor cloud ainda não foi escolhido.

## Decisão

Definir uma porta `IFileStorage`/serviços específicos na Application e adaptadores na Infrastructure. Metadados ficam no PostgreSQL; bytes ficam em object storage compatível. Fotos públicas aprovadas usam entrega controlada/CDN; saúde e evidências ficam privadas com URL temporária e autorização. Desenvolvimento usa storage local/emulador fora do repositório.

## Consequências

- Provedor pode ser escolhido depois sem contaminar o domínio.
- Upload exige tamanho máximo, validação de assinatura/tipo, nome aleatório, processamento seguro e lifecycle.
- Backup, antivírus/scan, thumbnails e retenção precisam de configuração operacional antes de produção.
