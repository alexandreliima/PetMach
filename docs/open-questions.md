# Perguntas em aberto

As respostas não são necessárias para aprovar toda a Fase 0, salvo quando marcadas como bloqueadoras para a fase indicada.

## Decisões confirmadas em 2026-07-13

- O produto, a solução e os namespaces usarão o nome **PetMach**.
- CI remota não será configurada por enquanto; gates equivalentes serão executados localmente.
- Docker não está disponível por enquanto. Testcontainers, PostgreSQL orquestrado e a experiência completa do Aspire ficarão pendentes até existir runtime compatível.
- Não há Mac build host por enquanto. Android será a primeira plataforma validada localmente; iOS continuará no projeto, mas seu build será validado futuramente.

## Antes da Fase 2 — Identidade

4. Qual provedor enviará e-mails transacionais? Em desenvolvimento pode ser usado capturador local.
5. Qual idade mínima do tutor e qual texto/versão inicial de termos e política de privacidade?
6. Exclusão deve ocorrer imediatamente ou após janela de recuperação? Quais dados têm retenção legal/antifraude?
7. Um usuário pode ser simultaneamente tutor, parceiro e moderador/admin?

### Defaults provisórios usados na implementação da Fase 2

- E-mails são capturados em arquivos locais em `backend/src/PetMach.Api/.dev-emails/` somente em Development. Produção falha de forma explícita até um provedor ser escolhido.
- A idade mínima provisória é 18 anos. Termos e privacidade usam a versão inicial `2026-07-14`; textos jurídicos ainda precisam ser fornecidos.
- A exclusão é uma anonimização imediata e irreversível da credencial nesta versão. A política legal de retenção ainda precisa ser definida antes de produção.
- Novos cadastros recebem somente o papel `Tutor`. Acúmulo de papéis não é oferecido por endpoint até a decisão do produto.

Esses defaults são seguros para desenvolvimento, mas não encerram as perguntas 4–7.

## Antes das Fases 3–5

8. Qual granularidade da distância exibida (faixas, quilômetros arredondados ou bairro/região)?
9. Qual raio máximo de descoberta e quais filtros são obrigatórios por padrão?
10. O like é enviado por um cão específico do tutor; como escolher o cão de origem quando o tutor tem vários?
11. Ao desfazer match, mensagens são retidas e apenas ocultadas, ou eliminadas após prazo?
12. Qual política de retenção e moderação de mensagens e evidências?

### Defaults provisórios usados na Fase 4

- O tutor escolhe explicitamente qual de seus cães será a origem da descoberta.
- Até a decisão sobre granularidade e permissão de localização, a API retorna somente cidade/estado conforme a privacidade do perfil; nenhum valor de distância é inventado.
- Desfazer match marca o vínculo como encerrado e mantém histórico. A visibilidade e retenção das mensagens serão definidas antes da Fase 5.

### Default provisório usado na Fase 5

- Mensagens são preservadas após unmatch ou bloqueio, mas deixam de ser acessíveis aos participantes. Nenhuma eliminação automática será implementada antes da definição da política de retenção e moderação.

## Antes da Fase 6 — Parceiros e reservas

13. O parceiro confirma manualmente toda reserva ou alguns espaços têm confirmação automática?
14. Como funcionam horários recorrentes, feriados, bloqueios e antecedência mínima/máxima?
15. Cancelamento possui prazo, penalidade ou apenas registro no MVP?
16. O valor é somente informativo e pago presencialmente? Haverá comprovante/status de pagamento?
17. Qual fuso rege reservas: sempre o do estabelecimento?

### Defaults provisórios usados na Fase 6

- Toda reserva exige confirmação manual do parceiro.
- O primeiro incremento usa janelas explícitas de disponibilidade; recorrência, feriados e regras de antecedência aguardam definição.
- Cancelamento não possui penalidade financeira no MVP inicial, mas registra ator e instante.
- Valores são informativos e o pagamento ocorre presencialmente; nenhum pagamento real será processado.
- Datas e horários do estabelecimento usam seu identificador IANA de fuso, persistindo eventos relevantes em UTC.

Esses defaults permitem desenvolvimento seguro, mas não encerram as perguntas 13–17 antes da produção.

## Antes da Fase 7 — Adoção e moderação

18. Quem pode publicar adoção: tutor, ONG, parceiro ou apenas entidade verificada?
19. O “pedido de adoção” obrigatório nos testes é uma candidatura formal? Quais campos e estados possui?
20. Quais ações cada motivo permite ao moderador e quais exigem administrador?
21. Quais prazos de retenção existem para denúncias, evidências e auditoria?

### Defaults provisórios usados na Fase 7

- No primeiro incremento, somente um tutor publica um cão próprio e ativo; ONGs e parceiros aguardam definição de verificação institucional.
- Cada cão possui no máximo uma publicação, separada integralmente de likes e matches.
- Publicação exige aceite explícito do termo de adoção responsável na versão `2026-07-16`.
- Candidaturas formais e seus campos/estados serão implementados no próximo incremento, sem presumir aprovação automática.

Esses defaults permitem o catálogo inicial, mas não encerram as perguntas 18–21 antes da produção.

## Antes de produção

22. Qual provedor de object storage, domínio, ambiente cloud e região de dados?
23. Quais métricas de produto e consentimento analítico serão adotados?
24. Quais SLAs, RPO/RTO, política de backup e canais de suporte são necessários?
