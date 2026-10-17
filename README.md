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
tests/
  AgapeApp.Domain.Tests
  AgapeApp.Application.Tests
  AgapeApp.Infrastructure.Tests
  AgapeApp.Api.IntegrationTests
```

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
