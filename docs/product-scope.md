# Escopo do produto

## Visão

PetMach é uma plataforma de socialização, experiências e serviços para cães e seus tutores. Match é um mecanismo de descoberta; reservas e serviços de parceiros constituem o principal eixo comercial.

## Usuários

- Tutor: gerencia conta e cães, descobre perfis, cria matches, conversa, agenda encontros, reserva e acessa adoções.
- Parceiro: gerencia estabelecimento, espaços, disponibilidade e reservas.
- Moderador: analisa denúncias e aplica ações limitadas de moderação.
- Administrador: gerencia plataforma, parâmetros e auditoria, com políticas específicas.

## Dentro do MVP

- Identidade com confirmação, recuperação, JWT curto, refresh rotativo, logout e exclusão.
- Tutores, cães, fotos, preferências, vacinas e registros de saúde protegidos.
- Descoberta por filtros e distância aproximada; likes, matches recíprocos e bloqueios.
- Chat textual persistido e em tempo real via SignalR.
- Encontros em locais parceiros.
- Parceiros, espaços, disponibilidade e reservas sem pagamento real.
- Área independente de adoção responsável.
- Notificações internas, denúncias, moderação, auditoria e painel administrativo.
- Aplicativo MAUI Android/iOS em português do Brasil, acessível e preparado para i18n.

## Fora do MVP

- Reprodução ou cruzamento.
- Pagamento real; será usado `IPaymentGateway` com implementação fake/presencial.
- Áudio, vídeo e chamadas no chat.
- Exposição de localização exata.
- Microserviços, broker e infraestrutura distribuída sem evidência de necessidade.
- Redis e PostGIS como dependências obrigatórias iniciais; permanecem opções avaliáveis.

## Premissas

- Brasil é o mercado inicial; LGPD e CNPJ orientam requisitos.
- Um tutor pode ter vários cães, mas interações de descoberta ocorrem entre perfis de cães.
- Um usuário autenticado pode acumular papéis quando autorizado (por exemplo, tutor e representante de parceiro).
- Todos os tempos persistidos são UTC; exibição usa o fuso do dispositivo/estabelecimento.
- Imagens começam atrás de abstração de storage e nunca são servidas como caminho público permanente sem política.
- Notificações internas são fonte de verdade no MVP; push será um adaptador futuro.

## Critérios de sucesso do primeiro release

- Tutor completa cadastro, cria cão e encontra perfis sem exposição de coordenadas.
- Like recíproco cria um único match e habilita conversa somente aos participantes.
- Encontro pode ser proposto e uma reserva pode ser confirmada sem conflito.
- Bloqueio interrompe descoberta e comunicação; denúncia entra no fluxo administrativo.
- Operações críticas são autorizadas, auditáveis e cobertas por testes.
