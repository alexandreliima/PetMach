# ADR-006 — Estratégia geoespacial e privacidade

- Status: Aceita
- Data: 2026-07-13

## Contexto

Descoberta requer distância e raio, enquanto LGPD e segurança proíbem expor a posição exata de outro tutor. O volume inicial é desconhecido.

## Decisão

Persistir coordenadas protegidas no backend e calcular distância exclusivamente no servidor. A API retorna apenas distância arredondada/faixa e região autorizada. Começar sem tornar PostGIS obrigatório; medir qualidade e desempenho das consultas. Adotar PostGIS por migration e ADR substituto se raio/índices espaciais forem necessários.

## Consequências

- Infraestrutura inicial mais simples e superfície de vazamento reduzida.
- Consultas básicas devem ser cuidadosamente indexadas e limitadas.
- Contratos e logs nunca incluem coordenadas; testes verificam serialização e autorização.
