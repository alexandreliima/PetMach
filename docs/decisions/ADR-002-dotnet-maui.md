# ADR-002 — .NET MAUI para o aplicativo mobile

- Status: Aceita
- Data: 2026-07-13

## Contexto

O produto exige Android e iOS, C#/XAML, integração com geolocalização, mídia, armazenamento seguro e uma equipe centrada em .NET.

## Decisão

Usar .NET MAUI 10 com MVVM, CommunityToolkit.Mvvm, Shell Navigation e DI nativa. ViewModels não dependem diretamente de APIs de plataforma; serviços encapsulam permissões, localização, arquivos e secure storage.

## Consequências

- Reuso de C# e ferramentas com o backend.
- Builds iOS exigem macOS/Mac build host e ecossistema Apple; como não há Mac disponível por enquanto, Android será validado primeiro e iOS ficará como gate futuro explícito.
- A máquina atual possui apenas workloads MAUI 9; a linha 10 deve ser instalada e validada na Fase 1.
