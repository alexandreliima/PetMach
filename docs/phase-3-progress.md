# Progresso — Fase 3

Data de início: 2026-07-14

## Incremento 1 — experiência visual e perfil do tutor

- Telas MAUI de boas-vindas, cadastro, login e início.
- Navegação Shell e ViewModels MVVM testáveis.
- Cadastro e login conectados à API; tokens armazenados no `SecureStorage`.
- HTTP para `10.0.2.2` permitido somente em Debug Android.
- Agregado `TutorProfile` com nome, telefone, cidade/estado, biografia e privacidade.
- `GET /api/v1/tutors/me` e `PUT /api/v1/tutors/me`, protegidos pela política `TutorAccess`.
- Ownership determinado pelo usuário autenticado; o cliente não escolhe o proprietário.
- Migration `Phase3TutorProfile`.

## Incremento 2 — cães, galeria e saúde

- Agregado `Dog` com ownership, estado do perfil e validação de dados.
- CRUD protegido em `/api/v1/dogs` e catálogo público em `/api/v1/dogs/breeds`.
- Fotos JPEG, PNG e WebP validadas pelo conteúdo real, com limite de 5 MB.
- Arquivos de desenvolvimento isolados em `.dev-storage`, fora do controle de versão.
- Registros protegidos de vacinas e vermífugos vinculados ao tutor e ao cão.
- Indicador de vacinação derivado e aviso de que o recurso não substitui orientação veterinária.
- Migration `Phase3DogsAndHealth`, com schemas e índices para cães, fotos e saúde.
- Prévia navegável no app para perfil do tutor, cães, formulário canino e carteira de saúde.
- A prévia não libera gravações anônimas; dados pessoais continuam exigindo autenticação.

## Validação atual

- Backend e Android compilados com 0 warnings e 0 erros.
- 41 testes aprovados, 0 falhas e 0 ignorados.
- Cobertura de domínio canino, cronologia dos cuidados, validação, rotas mobile e autorização.
- Formatador aprovado.

## Fechamento da implementação

- Formulários mobile de perfil, cães e saúde conectados à API autenticada.
- Seleção e envio de foto do cão pelo dispositivo, com limite de 5 MB.
- Comprovantes de vacinação em PDF, JPEG ou PNG, armazenados de forma privada e acessíveis somente pelo tutor proprietário.
- Estados de carregamento, vazio, sucesso, erro e autenticação obrigatória adicionados às telas.

A implementação da Fase 3 está encerrada. A aplicação das migrations e os testes de persistência em PostgreSQL real continuam pendentes porque Docker não está instalado neste ambiente.
