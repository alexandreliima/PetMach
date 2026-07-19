# PetMach Mobile Design System

## Objetivo

O Design System Mobile estabelece uma linguagem visual reutilizável para o PetMach sem acoplar apresentação, navegação ou regras de negócio. A primeira versão prioriza cuidado, confiança, leveza e organização com uma identidade adulta: coral como ação e vínculo afetivo, teal como segurança e off-white como base acolhedora.

Os recursos ficam em `frontend/src/PetMach.Mobile/Resources/Styles` e são carregados por `App.xaml`.

## Paleta

| Papel | Token | Uso |
|---|---|---|
| Marca e ação principal | `PetMachPrimary` | CTA, seleção e destaques |
| Pressionado ou ênfase | `PetMachPrimaryDark` | Variação escura do coral |
| Fundo de seleção | `PetMachPrimarySoft` | Chips e superfícies de ênfase |
| Confiança e navegação | `PetMachSecondary` | Ações secundárias e ícones |
| Ênfase secundária | `PetMachSecondaryDark` | Contraste sobre fundos claros |
| Fundo secundário | `PetMachSecondarySoft` | Avatares e destaques suaves |
| Fundo do aplicativo | `PetMachBackground` | Base off-white quente |
| Superfície | `PetMachSurface` | Cards, inputs e conteúdo elevado |
| Superfície discreta | `PetMachSurfaceMuted` | Agrupamentos e estados vazios |
| Texto principal | `PetMachTextPrimary` | Títulos e conteúdo prioritário |
| Texto secundário | `PetMachTextSecondary` | Descrições e metadados |
| Borda | `PetMachBorder` | Contornos de cards e controles |
| Sucesso | `PetMachSuccess` | Confirmação e resultado positivo |
| Aviso | `PetMachWarning` | Atenção sem bloqueio |
| Erro | `PetMachError` | Falha, validação e ação destrutiva |

Variações `Soft` existem para feedback em superfícies sem comprometer a legibilidade. Os aliases antigos (`Primary`, `Surface`, `Teal` e equivalentes) permanecem temporariamente para que as páginas ainda não migradas continuem funcionando.

Não adicione cores hexadecimais diretamente em páginas. Se uma cor nova representar um papel semântico legítimo, adicione-a em `Colors.xaml` e documente seu uso.

## Tipografia

São usadas fontes do sistema, evitando dependência externa e problemas de licença. Os estilos disponíveis são:

- `PetMachDisplayStyle`: assinatura visual e destaques grandes;
- `PetMachPageTitleStyle`: título de página;
- `PetMachSectionTitleStyle`: título de seção;
- `PetMachCardTitleStyle`: título de card;
- `PetMachBodyStyle`: texto corrido;
- `PetMachBodySmallStyle`: descrições compactas;
- `PetMachCaptionStyle`: metadados e legendas;
- `PetMachButtonTextStyle`: texto de ação;
- `PetMachInputTextStyle`: conteúdo de formulário;
- `PetMachErrorTextStyle`: validação e erro;
- `PetMachSuccessTextStyle`: confirmação.

Use o estilo apropriado em vez de repetir `FontSize`, `FontAttributes` e `TextColor`.

## Espaçamento e dimensões

`Spacing.xaml` oferece a escala `PetMachSpace4`, `8`, `12`, `16`, `20`, `24` e `32`. Também centraliza:

- raios pequeno, médio, grande e pill;
- altura de botão e input;
- alvo mínimo de toque de 48 unidades;
- espessura de borda;
- paddings de input, card e chip.

Layouts devem combinar valores dessa escala. Medidas específicas de imagens e ilustrações podem continuar locais quando não representam um token reutilizável.

## Componentes e estilos

### Controles

- `PetMachPrimaryButtonStyle`: ação principal da tela;
- `PetMachSecondaryButtonStyle`: ação alternativa contornada;
- `PetMachGhostButtonStyle`: ação de baixa ênfase;
- `PetMachDestructiveButtonStyle`: logout e ações destrutivas;
- `PetMachInputContainerStyle` e `PetMachInputEntryStyle`: formulário padrão;
- `PetMachCardStyle` e `PetMachStrongCardStyle`: superfícies de conteúdo;
- `PetMachChipStyle` e `PetMachSelectedChipStyle`: filtros, tags e seleção;
- `PetMachAvatarBorderStyle`: avatar com placeholder;
- `PetMachLoadingIndicatorStyle`: indicador de atividade.

### Componentes reutilizáveis

- `SectionHeaderView`: eyebrow, título e descrição de uma seção;
- `PetAvatarView`: imagem circular com placeholder da marca;
- `StateView`: estado carregando, vazio, erro, sucesso ou aviso.

Exemplo:

```xml
<components:SectionHeaderView
    Eyebrow="PETMACH"
    Title="Meus pets"
    Subtitle="Gerencie seus companheiros." />

<components:StateView
    Kind="Empty"
    Title="Nenhum pet cadastrado"
    Message="Cadastre seu primeiro pet para começar." />
```

## Estados visuais

| Estado | Recurso |
|---|---|
| Carregando | `PetMachLoadingIndicatorStyle` ou `StateView Kind="Loading"` |
| Vazio | `PetMachEmptyStateStyle` ou `StateView Kind="Empty"` |
| Erro | `PetMachErrorStateStyle`, `PetMachErrorTextStyle` ou `StateView Kind="Error"` |
| Sucesso | `PetMachSuccessStateStyle`, `PetMachSuccessTextStyle` ou `StateView Kind="Success"` |
| Aviso | `PetMachWarningStateStyle` ou `StateView Kind="Warning"` |
| Validação | `PetMachErrorTextStyle` junto ao campo relacionado |
| Ação destrutiva | `PetMachDestructiveButtonStyle` |
| Item selecionado | `PetMachSelectedChipStyle` |
| Item desabilitado | trigger de `IsEnabled=False` no estilo base de botão |

Mensagens de estado devem explicar o que ocorreu e, quando aplicável, indicar a próxima ação.

## Aplicação inicial

Esta versão aplica a fundação visual somente em:

- `MainPage`: landing pública e onboarding;
- `LoginPage`: formulário e hierarquia de ações;
- `HomePage`: cabeçalho, ação principal, cards e logout;
- `DogsPage`: estado vazio, loading, cards e feedback de erro.

As demais páginas mantêm a composição atual e recebem apenas os estilos implícitos básicos e aliases de compatibilidade. Isso permite uma migração incremental sem alterar navegação, bindings ou ViewModels.

## Regras para novas telas

1. Use tokens semânticos; não replique hexadecimais.
2. Use a escala de espaçamento e os estilos tipográficos existentes.
3. Mantenha uma ação principal clara por contexto.
4. Use os componentes de estado para loading, vazio, erro e sucesso.
5. Preserve alvo de toque mínimo, contraste e texto legível.
6. Não registre componentes ou páginas visuais como singleton.
7. Não coloque navegação, sessão ou regras de negócio em componentes visuais.
8. Adicione um novo token somente quando o papel visual for reutilizável.

## Próximos passos

- migrar as páginas restantes por fluxo, começando por descoberta, match e encontros;
- substituir aliases de compatibilidade por tokens `PetMach*`;
- padronizar ícones com assets vetoriais próprios e licenciados;
- validar contraste com ferramenta automatizada;
- adicionar testes de apresentação para componentes com comportamento;
- revisar temas escuro e de alto contraste em incremento próprio.
