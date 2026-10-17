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

4. Swagger: http://localhost:5180/swagger

Na primeira execução a API:

- aplica as **migrations** pendentes (FluentMigrator — somente Up/forward-only);
- cria o **primeiro administrador** conforme a seção `PrimeiroAdministrador` do appsettings
  (padrão: `admin@agapeapp.local` / `Agape@123456` — **troque a senha depois**).

## Endpoints (v1)

| Método | Rota | Acesso |
| --- | --- | --- |
| POST | `/api/autenticacao/login` | Público |
| POST | `/api/autenticacao/renovar` | Público |
| POST | `/api/autenticacao/sair` | Autenticado |
| GET | `/api/autenticacao/eu` | Autenticado |
| POST | `/api/usuarios` | Administrador |
| GET | `/api/usuarios` | Administrador |

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
  AgapeApp.Infrastructure  → EF Core + SQL Server, FluentMigrator, Argon2, JWT
  AgapeApp.Api             → controllers, autenticação, Swagger
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
- **Chave JWT:** o valor de `Autenticacao:ChaveSecreta` é derivado via SHA-256 para virar a chave de assinatura —
  funciona com qualquer tamanho de segredo (em produção use um segredo longo e aleatório).
- **Datas:** armazenadas em UTC (`datetimeoffset`).
