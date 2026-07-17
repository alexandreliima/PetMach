# Riscos e mitigação

| Risco | Probabilidade/impacto | Mitigação inicial |
|---|---|---|
| SDK .NET 10 e MAUI 10 ausentes | Alta/Alta | Instalar e validar antes da Fase 1; fixar SDK real em `global.json` |
| Desenvolvimento iOS sem Mac disponível | Alta/Alta | Implementar arquitetura multiplataforma, validar Android primeiro e registrar build/assinatura iOS como gate futuro |
| Docker indisponível em alguma estação/runner | Média/Alta | A fixture Testcontainers falha explicitamente, sem falso verde; documentar Docker como pré-requisito e manter Compose reproduzível |
| CI remota adiada | Média/Média | Fornecer script local reproduzível com restore, format, build, test e cobertura; escolher provedor antes do release |
| Escopo do MVP muito amplo | Alta/Alta | Entregas verticais por fase, flags e critérios de aceite; priorizar caminho cadastro→match→reserva |
| Privacidade de localização | Média/Alta | Cálculo exclusivo no servidor, arredondamento, autorização e testes contra vazamento |
| Saúde e evidências sensíveis | Média/Alta | Private-by-default, storage protegido, URLs temporárias, auditoria e retenção definida |
| Dupla reserva concorrente | Média/Alta | Transação, constraint no PostgreSQL, concorrência otimista e testes simultâneos |
| Abuso no chat/adoção | Alta/Alta | Bloqueio imediato, denúncia, rate limit, moderação e trilha de auditoria |
| Refresh token roubado/reutilizado | Média/Alta | Hash no banco, rotação, revogação em família e detecção de reuse |
| Monólito virar “bola de lama” | Média/Alta | Limites por módulo, schemas/mapeamentos, contratos e testes arquiteturais |
| Testcontainers indisponível ou instável | Média/Média | Imagem PostgreSQL fixada, container por coleção, timeout explícito, descarte garantido e falha visível quando o daemon não está acessível |
| Imagens elevarem custo/risco | Alta/Média | Limites, compressão, thumbnails, abstração de storage e lifecycle policy |
| Regras de parceiros/reservas indefinidas | Alta/Alta | Resolver perguntas comerciais antes da Fase 6; não codificar suposições críticas |
| Dependência de app stores e consentimentos | Média/Alta | Preparar textos, políticas, contas e revisão de permissões cedo |
| Build MAUI tornar CI lenta | Média/Média | Separar jobs e plataformas; testes de ViewModels rápidos e builds mobile direcionados |

## Risco de cronograma

Tratar as 30 telas, API, Admin, Identity, SignalR, geolocalização, storage, reservas e moderação como um único release indivisível elevaria muito o risco. O backlog propõe marcos demonstráveis e mantém o monólito implantável em cada fase.
