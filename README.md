# AgapeApp — API

API do **AgapeApp**: cartão pré-pago e gestão de eventos da igreja.
As decisões do projeto (domínio e técnicas) estão em [`plano-projeto-eventos.md`](plano-projeto-eventos.md).

## Requisitos

- .NET SDK 10
- SQL Server local (exemplo: instância `localhost\MSSQLSERVER01`)

## Como rodar

1. Use o banco `dbAgape` no seu SQL Server local
   (o exemplo aponta para a instância `localhost\MSSQLSERVER01`).

2. Se precisar, ajuste a conexão em `src/AgapeApp.Api/appsettings.json` (ou no `appsettings.Development.json`):

   ```json
   "ConnectionStrings": {
     "SqlServer": "Server=localhost\\MSSQLSERVER01;Database=dbAgape;Trusted_Connection=True;TrustServerCertificate=True"
   }
   ```

3. Execute:

   ```
   dotnet run --project src/AgapeApp.Api
   ```

4. O Swagger **abre automaticamente** no navegador em desenvolvimento
   (em `http://localhost:5180/swagger`); a raiz `http://localhost:5180/` também redireciona para ele.
   Para não abrir o navegador, use `"Swagger": { "AbrirNavegador": false }` no `appsettings.json`.

Na primeira execução a API:

- aplica as **migrations** pendentes (FluentMigrator — somente Up/forward-only);
- cria o **primeiro administrador** conforme a seção `PrimeiroAdministrador` do appsettings
  (padrão: `admin@agapeapp.local` / `Agape@123456` — **troque a senha depois**).

## Endpoints (v1)

### Autenticação e usuários

| Método | Rota | Acesso |
| --- | --- | --- |
| POST | `/api/autenticacao/login` | Público |
| POST | `/api/autenticacao/renovar` | Público |
| POST | `/api/autenticacao/sair` | Autenticado |
| GET | `/api/autenticacao/eu` | Autenticado |
| POST | `/api/autenticacao/alterar-senha` | Autenticado (exige a senha atual) |
| POST | `/api/autenticacao/minha-foto` | Autenticado (só a própria foto; `multipart/form-data`, campo `arquivo`) |
| DELETE | `/api/autenticacao/minha-foto` | Autenticado (só a própria foto) |
| POST | `/api/usuarios` | Administrador |
| GET | `/api/usuarios` | Administrador |
| PUT | `/api/usuarios/{id}` | Administrador |
| POST | `/api/usuarios/{id}/ativar` · `/desativar` | Administrador |
| POST | `/api/usuarios/{id}/redefinir-senha` | Administrador |

### Cadastros (eventos, setores, produtos, cartões e vínculos)

| Método | Rota | Acesso |
| --- | --- | --- |
| POST · GET | `/api/produtos` | Administrador |
| GET · PUT | `/api/produtos/{id}` | Administrador |
| POST | `/api/produtos/{id}/ativar` · `/desativar` | Administrador |
| POST · GET | `/api/cartoes` | Administrador |
| GET | `/api/cartoes/{id}` | Administrador |
| POST | `/api/cartoes/{id}/ativar` · `/desativar` | Administrador |
| POST · GET | `/api/eventos` | Administrador |
| GET · PUT | `/api/eventos/{id}` | Administrador |
| GET | `/api/eventos/meus` | Autenticado (operador) |
| POST | `/api/eventos/{id}/abrir` · `/encerrar` | Administrador |
| POST · GET | `/api/eventos/{eventoId}/setores` | Administrador |
| PUT | `/api/setores/{id}` | Administrador |
| POST · GET | `/api/eventos/{eventoId}/produtos` | Administrador |
| PUT | `/api/evento-produtos/{id}` | Administrador |
| POST | `/api/evento-produtos/{id}/ajuste-estoque` | Administrador |
| POST | `/api/setores/{id}/produtos` | Administrador |
| DELETE | `/api/setores/{id}/produtos/{eventoProdutoId}` | Administrador |
| GET | `/api/setores/{id}/produtos` | Autenticado (operador) |
| POST · GET | `/api/eventos/{eventoId}/operadores` | Administrador |
| PUT | `/api/evento-operadores/{id}` | Administrador |

### Operações (recarga e venda)

| Método | Rota | Acesso |
| --- | --- | --- |
| POST | `/api/recargas` | Autenticado (operador ou administrador) |
| GET | `/api/cartoes/{codigo}/saldo?eventoId={id}` | Autenticado |
| GET | `/api/eventos/{eventoId}/recargas` | Administrador |
| POST | `/api/vendas` | Autenticado (operador ou administrador) |
| GET | `/api/eventos/{eventoId}/vendas` | Administrador |
| GET | `/api/vendas/{id}` | Administrador |

### Relatórios e tempo real

| Método | Rota | Acesso |
| --- | --- | --- |
| GET | `/api/eventos/{eventoId}/relatorios` | Administrador |
| GET | `/api/eventos/{eventoId}/relatorios/saldos` | Administrador |
| GET | `/api/eventos/{eventoId}/relatorios/excel` | Administrador |
| GET | `/api/eventos/{eventoId}/auditoria` | Administrador |
| WS | `/hubs/dashboard` (SignalR) | Administrador |

Exemplo de login:

```json
POST /api/autenticacao/login
{
  "email": "admin@agapeapp.local",
  "senha": "Agape@123456",
  "dispositivo": "PC do admin"
}
```

Resposta: `tokenAcesso`, `tokenRenovacao`, `expiraEm` e `usuario`.

### Exemplo de operação idempotente (recarga)

O app gera um `idOperacaoCliente` (Guid) por operação. Se a resposta se perder (rede instável) e o app reenviar a mesma operação, a API devolve o registro original — o saldo **não** duplica:

```json
POST /api/recargas
{
  "cartaoCodigo": "CARD-0001",
  "eventoId": "…",
  "setorId": "…",
  "valor": 50,
  "idOperacaoCliente": "…",
  "dispositivo": "Celular do caixa 1"
}
```

A venda (`POST /api/vendas`) segue o mesmo formato, trocando `valor` por `itens` (`eventoProdutoId` + `quantidade`), valida o saldo **e** o estoque e grava o preço praticado no momento (snapshot).

## Testando endpoints protegidos pelo Swagger

1. Com a API rodando, faça `POST /api/autenticacao/login` (Try it out → Execute).
2. Copie **somente o valor** de `tokenAcesso` — sem aspas e **sem** o prefixo `Bearer`.
3. Clique em **Authorize** (topo da página), cole o valor, confirme e feche.
4. Agora os endpoints protegidos enviam o cabeçalho automaticamente. Exemplo — `POST /api/usuarios`:

   ```json
   {
     "nome": "Maria Silva",
     "email": "maria@igreja.local",
     "senha": "Senha@123456",
     "perfil": "Operador"
   }
   ```

   O token de acesso dura 30 minutos; se expirar, faça login novamente.

Se vier `401`, a resposta diz o motivo: token **não informado**, **expirado** ou **inválido**.

## Testes

```
dotnet test
```

## Estrutura

```
AgapeApp.slnx
src/
  AgapeApp.Domain          → entidades e regras de negócio
  AgapeApp.Application     → casos de uso (CQRS/MediatR), validações (FluentValidation)
  AgapeApp.Infrastructure  → EF Core + SQL Server, FluentMigrator, Argon2, JWT, export Excel
  AgapeApp.Api             → controllers, autenticação, SignalR, Swagger
  AgapeApp.Mobile          → app .NET MAUI (Android + iPhone) — build próprio, fora da .slnx
tests/
  AgapeApp.Domain.Tests
  AgapeApp.Application.Tests
  AgapeApp.Infrastructure.Tests
  AgapeApp.Api.IntegrationTests
```

## App mobile (AgapeApp.Mobile)

App do operador em **.NET MAUI** — um único projeto para **Android e iPhone** (`src/AgapeApp.Mobile`).
Fica **fora** da `AgapeApp.slnx` de propósito: o build/teste da API não depende do toolchain mobile (útil no dia do deploy).

```
dotnet build src/AgapeApp.Mobile -f net10.0-android   # APK autônomo em bin/Debug/net10.0-android
dotnet build src/AgapeApp.Mobile -f net10.0-ios       # requer Mac (ou Mac pareado)
```

O build de Debug já gera um **APK autônomo** (as DLLs vão embutidas; `EmbedAssembliesIntoApk` no `.csproj`).
Para instalar no aparelho conectado por USB:

```
adb install -r src/AgapeApp.Mobile/bin/Debug/net10.0-android/com.agapeapp.mobile-Signed.apk
```

O APK fica em torno de **93 MB** no Debug porque leva as arquiteturas **arm64-v8a** (aparelho) e
**x86_64** (emulador) com símbolos. Com o aparelho plugado, `dotnet build src/AgapeApp.Mobile -f net10.0-android -t:Run`
compila, instala e abre o app de uma vez.

- **Design:** design system "Agape Direct" (Material 3) gerado no Google Stitch, salvo em `docs/design/stitch/`.
  Tokens aplicados em `Resources/Styles/Colors.xaml` e `Styles.xaml` (primária `#2563EB`, dinheiro `#16A34A`,
  recarga `#F59E0B`, fonte Inter, botões 52px/raio 12, cards raio 16).
- **Telas:** login (com biometria), meus eventos (busca + filtros; o admin alterna entre os dele e todos),
  operação do evento (com as últimas operações deste aparelho; o admin escolhe o setor antes de abrir),
  leitura do QR do cartão **pela câmera**, recarga (valores rápidos + valor livre), confirmação de recarga,
  venda (catálogo do setor com quantidades), confirmação de venda, saldo do cartão, perfil (com a **foto de perfil**)
  e troca de senha, além das telas do **administrador**: dashboard do evento (totais + ao vivo pelo SignalR),
  saldos em circulação e auditoria paginada.
- **Aba Gestão (só para o administrador):** o operador vê apenas Eventos e Perfil; o admin ganha a aba **Gestão**
  com usuários, produtos, eventos e cartões (criar/editar/ativar/desativar). Em **detalhes do evento**: setores,
  produtos do evento (vincular ao setor, preço e estoque com **entrada** e **baixa**) e operadores designados,
  além de **abrir/encerrar** o evento. O admin também pode **operar o terminal** em qualquer setor de qualquer
  evento aberto (regra que já existe na API).
- **Câmera e biometria:** leitura do QR/código de barras com `ZXing.Net.Maui.Controls` (lanterna e digitação
  manual como reserva); entrada por biometria com `Plugin.Maui.Biometric` — ativada no primeiro login,
  ligável/desligável no perfil e desligada ao sair da conta. `minSdk` do Android: **26** (exigência do AndroidX/biometria).
- **Cache local:** SQLite (`sqlite-net-pcl`) guarda eventos do operador, produtos do setor e as últimas operações
  deste aparelho; sem internet, as listas aparecem com o aviso "dados salvos" (a **escrita continua exigindo internet**).
- **Arquitetura:** MVVM (`CommunityToolkit.Mvvm`), Shell para navegação, `ApiClient` + `SessaoService`
  (tokens no SecureStorage com renovação automática); mensagens de erro vêm da ProblemDetails da API, já em português.
  O dashboard usa `Microsoft.AspNetCore.SignalR.Client` no hub `/hubs/dashboard` (evento `operacao`).
- **Endereço da API:** o app usa por padrão o **túnel ngrok** que expõe a API local em HTTPS
  (`AppConfig.ApiBaseUrlPadrao`, em `src/AgapeApp.Mobile/Config/AppConfig.cs`) — funciona no aparelho físico em
  qualquer rede, sem liberar firewall. Fluxo: rode a API local (`dotnet run --project src/AgapeApp.Api`) e, em
  outro terminal, `ngrok http 5180`; se o endereço do túnel mudar, atualize a constante **ou** toque em
  **"Configurar endereço do servidor"** na tela de login (só em DEBUG), que grava a URL no aparelho.
  Sem ngrok (mesma rede): emulador Android `http://10.0.2.2:5180`, simulador iOS `http://localhost:5180`,
  aparelho físico `http://IP-DO-PC:5180` com a porta liberada:

  ```
  dotnet run --project src/AgapeApp.Api -- --urls "http://0.0.0.0:5180"
  ```
- **Idempotência:** retries de recarga e venda reenviam o mesmo `idOperacaoCliente` — nada é cobrado duas vezes.
- **Pendências do app:** validar em aparelho físico (câmera, biometria, SignalR, foto de perfil e as telas de
  gestão), build iOS (requer Mac) e o painel web do administrador (futuro, mesma API).

## Observações

- **Convenções de código:** estrutura/pastas e sufixos técnicos em inglês (`Domain`, `Controllers`, `UsuarioController`, `UsuarioDTO`, `LoginCommand`); domínio e métodos em português (`Usuario`, `Criar`, `GerarTokenAcesso`).
- **Migrations:** ficam em `src/AgapeApp.Infrastructure/Migrations` e rodam na inicialização da API.
  São **somente Up** (por decisão do projeto); o `Down` lança `NotSupportedException` de propósito.
  Para reverter algo, faça manualmente no banco ou crie uma migration de correção.
- **Senhas:** hash com **Argon2id** (64 MB, 3 iterações, 4 threads).
- **Sessões:** token de acesso JWT (padrão: 30 min) + token de renovação com rotação (padrão: 7 dias).
- **Senha e desativação:** redefinir senha (admin), trocar a própria senha e desativar um usuário revogam os
  tokens de renovação dele; o token de acesso atual segue valendo até expirar (30 min).
- **Chave JWT:** o valor de `Autenticacao:ChaveSecreta` é derivado via SHA-256 para virar a chave de assinatura —
  funciona com qualquer tamanho de segredo (em produção use um segredo longo e aleatório).
- **Datas:** armazenadas em UTC (`datetimeoffset`).
- **Fotos de perfil:** gravadas em `src/AgapeApp.Api/wwwroot/imagens/` (coluna `caminho_foto`, migration 0017)
  e servidas como arquivos estáticos em `{apiBase}/imagens/{arquivo}`. Aceita jpg/jpeg/png/webp até 5 MB
  (tipo conferido pelos *magic bytes*, não pela extensão); só o próprio usuário troca/remove a sua foto.
- **Mensagens de erro:** centralizadas em `src/AgapeApp.Domain/Resources/MensagensErro.resx`
  (acesso tipado por `MensagensErro.NomeDaChave`), usadas pelo domínio, validators, handlers e tratador global.
  Campos ausentes e falhas de JSON/binding também devolvem ProblemDetails em português.
  Mensagens de log e de configuração/startup continuam no código (não são devolvidas ao cliente).
- **Erros de autenticação:** 401/403 do middleware também voltam em ProblemDetails, em português
  (token não informado, expirado ou inválido; permissão insuficiente).
- **Idempotência:** recargas e vendas têm `id_operacao_cliente` com índice único — retry nunca duplica.
- **Tempo real:** o hub SignalR `/hubs/dashboard` avisa o dashboard do admin quando uma operação é registrada
  (evento client `operacao`, grupo `evento-{eventoId}`). No WebSocket o token vai na query string: `?access_token=…`.
- **Excel:** o relatório do evento é gerado com **ClosedXML** (abas: Resumo, Por setor, Por operador, Por produto e Saldos em circulação).
