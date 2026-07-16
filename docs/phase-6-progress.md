# Progresso — Fase 6

Data de início: 2026-07-15

## Incremento 1 — parceiros e espaços

- Estabelecimento parceiro com representante proprietário, identificação empresarial, nome público, cidade, estado e fuso.
- CNPJ/registro empresarial permanece somente no contrato de gestão e não aparece no catálogo.
- Um estabelecimento por representante e registro empresarial único.
- Espaços com capacidade, descrição e valor meramente informativo.
- Gestão protegida por `PartnerAccess` e ownership.
- Catálogo autenticado, filtrável por cidade e estado, limitado a 100 resultados.
- Tela mobile inicial para consulta de espaços.
- Nenhuma coordenada exata ou pagamento real.

## Incremento 2 — disponibilidade

- Janelas futuras explícitas por espaço, com duração máxima de sete dias.
- Datas persistidas e contratadas em UTC; o mobile apresenta no horário local do dispositivo.
- Criação restrita ao proprietário do estabelecimento por `PartnerAccess` e ownership.
- Sobreposição com outra janela ativa é rejeitada com conflito HTTP 409.
- Consulta autenticada cobre por padrão os próximos 30 dias e limita intervalos solicitados a 90 dias.
- Catálogo mobile permite selecionar um espaço e visualizar os horários disponíveis.

## Incremento 3 — reservas pendentes

- Tutor solicita uma reserva para um cão próprio em uma janela futura disponível.
- Toda reserva começa como `Pending` e exige confirmação manual do parceiro proprietário.
- Uma restrição parcial única no PostgreSQL impede mais de uma reserva `Pending` ou `Confirmed` por janela.
- Listagens são separadas por ownership: tutor vê somente as próprias reservas e parceiro somente reservas dos seus espaços.
- Confirmação repetida ou horário já ocupado retorna conflito HTTP 409.

## Incremento 4 — cancelamento e atendimento presencial

- Tutor ou parceiro proprietário pode cancelar reservas `Pending` ou `Confirmed`, sem penalidade financeira.
- Cancelamento preserva ator e instante UTC na reserva e no histórico imutável de transições.
- Parceiro pode concluir uma reserva confirmada após o início da janela ou registrar ausência após seu término.
- Pagamento permanece externo ao PetMach; registra-se somente `AwaitingOnSite` ou `RecordedOnSite`, sem valor transacionado ou dados financeiros.
- Tutor e parceiro consultam o histórico somente dentro do respectivo ownership.

## Incremento 5 — reservas no mobile

- Tela própria acessível pela home reúne solicitação e acompanhamento das reservas do tutor.
- Tutor seleciona um cão próprio, espaço e janela disponível antes de solicitar.
- Estados do servidor são apresentados em português sem alterar o contrato persistido.
- Reservas `Pending` ou `Confirmed` podem ser canceladas diretamente na lista.
- Falhas de concorrência são apresentadas como horário indisponível, mantendo o servidor como fonte de verdade.

## Incremento 6 — operação do parceiro no mobile

- Painel do parceiro consulta somente o estabelecimento e os espaços sob seu ownership.
- Novas janelas são informadas no horário do estabelecimento e convertidas para UTC usando o fuso IANA cadastrado.
- Reservas recebidas podem ser confirmadas, canceladas, concluídas ou marcadas como ausência conforme as regras do servidor.
- Conclusão permite registrar apenas a confirmação informativa de pagamento presencial.
- Endpoints `partners/me` e `partners/me/spaces` evitam expor dados de gestão de outros parceiros.

## Gate PostgreSQL real

- Todas as migrations foram aplicadas do zero no PostgreSQL 18.
- A validação real identificou e corrigiu a ordem dos argumentos de foreign keys na migration consolidada de reservas.
- Duas transações concorrentes disputando a mesma disponibilidade resultam em uma gravação e uma violação única `23505`.
- O banco foi limpo após os cenários e a suíte permanece determinística por IDs exclusivos.

## Próximos incrementos

1. Evoluções futuras de recorrência e regras comerciais após decisão de produto.
2. Automação equivalente por Testcontainers quando o runtime permitir acesso ao Docker.

A Fase 6 está concluída.
