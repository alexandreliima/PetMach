# Frontend

Frontend mobile do PetMach em .NET MAUI 10.

- `PetMach.Mobile.Core`: ViewModels, clientes HTTP, sessão e lógica de
  navegação independentes da plataforma.
- `PetMach.Mobile`: páginas XAML/MAUI, Shell autenticada, `SecureStorage`,
  SignalR e hosts Android/iOS.
- `PetMach.Mobile.Tests`: testes rápidos da camada mobile compartilhada,
  incluindo regressões de sessão, refresh e troca de raiz.

Android é o alvo validado localmente. O TFM iOS é incluído automaticamente quando o build roda em macOS.

## Sessão e navegação

Páginas e `AppShell` são transientes. `RootNavigationService` é o único ponto
de troca da raiz da janela: autenticação cria uma nova `AppShell`; logout ou
sessão inválida criam uma nova raiz pública. Tokens são persistidos somente
pela implementação `SecureTokenStore`.

Chamadas protegidas compartilham uma única renovação quando necessário e são
repetidas no máximo uma vez depois de `401`. Login e refresh usam o cliente de
autenticação separado.

## API no Android

Debug usa `http://10.0.2.2:5049/` por padrão para alcançar a API no host do
emulador. Uma URL absoluta alternativa pode ser fornecida por
`PETMACH_API_BASE_URL`; essa variável contém endpoint, não segredo.

Com API e PostgreSQL ativos:

```powershell
dotnet build src/PetMach.Mobile/PetMach.Mobile.csproj -f net10.0-android -t:Run
```

Consulte [operação](../docs/operations.md) e
[testes](../docs/testing.md).
