# Guia didático — Fase 4, incremento 1

> Registro histórico do primeiro incremento da Fase 4. Para o estado atual,
> consulte [Estado técnico atual](current-state.md) e
> [Guia didático do PetMach](guia-do-projeto.md).

## Resultado do incremento

O PetMach agora possui o primeiro fluxo de descoberta e match conectado ao aplicativo:

```text
Tutor escolhe seu cão
        ↓
API procura cães compatíveis
        ↓
Tutor curte ou ignora
        ↓
Existe like no sentido contrário?
   ├── Não → like fica aguardando
   └── Sim → match é criado
```

Também é possível listar e desfazer matches e bloquear o tutor de outro cão.

## O que foi integrado da Fase 3

Antes da descoberta, foram concluídas as conexões mobile com a API para:

- perfil do tutor;
- cães;
- fotos;
- vacinas;
- vermífugos;
- comprovantes de vacinação.

Os comprovantes aceitam PDF, JPEG e PNG, possuem limite de 5 MB, ficam em armazenamento privado e só podem ser acessados pelo tutor proprietário do cão.

## Descoberta

A descoberta sempre parte de um cão pertencente ao tutor autenticado. Esse cão é chamado de `SourceDog`, ou cão de origem.

```text
DiscoveryPage
   ↓ utiliza
DiscoveryViewModel
   ↓ chama o cliente HTTP autenticado
GET /api/v1/discovery?sourceDogId=...
   ↓
DiscoveryController
   ↓
DiscoveryService
   ↓ consulta EF Core
PostgreSQL
```

A resposta é paginada e contém somente os dados permitidos para apresentação.

## Quem não aparece na descoberta

O servidor remove automaticamente:

- cães do próprio tutor;
- cães com perfil inativo;
- tutores que desativaram a descoberta;
- cães já curtidos pelo cão de origem;
- cães já ignorados pelo cão de origem;
- cães pertencentes a usuários bloqueados em qualquer direção.

Essa filtragem acontece no servidor. O Mobile não recebe os registros proibidos para depois escondê-los.

## Filtros implementados no backend

O backend aceita filtros por:

- sexo;
- porte;
- raça;
- nível de energia;
- objetivo no aplicativo;
- castração;
- vacinação atualizada.

O filtro completo e a paginação incremental na interface mobile ainda pertencem ao próximo incremento.

## Privacidade da localização

Nenhuma latitude ou longitude exata é devolvida pela API. A descoberta mostra somente a região autorizada pelo tutor:

- cidade e estado, quando o tutor permite mostrar a cidade;
- somente estado, quando a cidade está oculta.

O filtro de distância ainda não foi ativado. Primeiro será necessário armazenar a localização de forma protegida e calcular a distância exclusivamente no servidor.

## Curtir

O like é direcionado entre dois cães:

```text
Cão A → curte → Cão B
```

O servidor verifica:

- se o cão A pertence ao usuário autenticado;
- se os dois cães estão ativos;
- se pertencem a tutores diferentes;
- se não existe bloqueio;
- se o perfil de destino participa da descoberta;
- se o cão não foi ignorado anteriormente.

O banco possui restrições contra auto-like e duplicidade.

## Match recíproco

Um match só é criado quando existem likes nos dois sentidos:

```text
Cão A → like → Cão B
Cão B → like → Cão A
              ↓
            Match
```

Os IDs são colocados em uma ordem canônica antes da gravação. Assim, `A+B` e `B+A` representam o mesmo par. Um índice único no PostgreSQL impede matches duplicados.

A criação ocorre dentro de uma transação com isolamento serializável para reduzir problemas quando os likes chegam simultaneamente.

## Ignorar

Ignorar cria um registro de pass entre o cão de origem e o cão visualizado. Depois disso, o perfil não volta nas próximas páginas para aquele cão de origem.

## Desfazer match

Desfazer não apaga o histórico. O match recebe uma data de encerramento em UTC. Isso preserva rastreabilidade e permite que regras futuras saibam que a relação existiu.

## Bloqueio

O bloqueio atua entre tutores, embora a ação comece a partir do perfil de um cão:

```text
Tutor A bloqueia o dono do Cão B
        ↓
Perfis ficam ocultos nos dois sentidos
        ↓
Matches ativos entre os tutores são encerrados
```

O bloqueio bilateral significa que nenhum dos dois lados deve continuar encontrando ou interagindo com o outro.

## Fotos na descoberta

A chave física do arquivo não é exposta. A foto principal é entregue por um endpoint autenticado:

```http
GET /api/v1/discovery/dogs/{dogId}/photo
```

Antes de servir o arquivo, a API verifica perfil ativo, permissão de descoberta e bloqueios.

## Telas mobile

Foram adicionadas rotas para:

- `discovery` → `DiscoveryPage`;
- `matches` → `MatchesPage`.

As telas estão conectadas à API e já podem ser executadas no emulador. A experiência de filtros completos, distância e paginação incremental ainda será ampliada.

## Persistência

A migration `Phase4DiscoveryMatches` cria estruturas para:

- likes;
- passes;
- matches;
- preferências de descoberta;
- usuários bloqueados.

Também cria constraints e índices para impedir auto-interação e duplicidade.

## Validação registrada

O relatório `docs/phase-4-progress.md` e a atualização fornecida registram:

- 53 testes aprovados;
- build completo com zero erros;
- build completo com zero avisos;
- migration `Phase4DiscoveryMatches` criada;
- aplicativo atualizado e executando no emulador.

A presença dos principais serviços, controllers, telas, rotas e migration foi inspecionada durante a atualização deste guia. O gate não foi executado novamente nesta sessão.

## Limitações atuais

- Os testes com PostgreSQL real e Testcontainers permanecem pendentes porque Docker não está instalado.
- A auditoria online de vulnerabilidades do NuGet não foi concluída porque o serviço externo não estava acessível.
- Localização protegida e filtro por distância ainda não foram implementados.
- Os filtros completos ainda não possuem toda a experiência visual mobile.
- A notificação interna de novo match ainda será implementada.

Essas limitações não anulam o build e os testes locais, mas continuam sendo gates necessários antes de produção.

## Estado atualizado

| Área | Estado atual |
|---|---|
| Fase 2 — Identity | Concluída |
| Fase 3 — tutor, cães e saúde | Integrada; PostgreSQL real ainda pendente |
| Descoberta paginada | Implementada |
| Filtros no backend | Implementados, exceto distância |
| Like e pass | Implementados |
| Match recíproco | Implementado |
| Desfazer match | Implementado com histórico |
| Bloqueio | Implementado |
| Telas Discovery e Matches | Implementadas e executando no emulador |
| Localização e distância | Próximo incremento |
| Filtros completos no mobile | Próximo incremento |
| Notificação de match | Próximo incremento |
| Fase 4 completa | Em andamento |

## Próximo incremento

1. Persistir localização protegida.
2. Calcular distância somente no servidor.
3. Exibir somente distância aproximada ou região permitida.
4. Criar filtros completos no Mobile.
5. Implementar paginação incremental.
6. Criar notificação interna de novo match.
7. Adicionar testes concorrentes com PostgreSQL real quando Docker estiver disponível.
