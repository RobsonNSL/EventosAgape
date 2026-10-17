# AgapeApp — API

API do **AgapeApp**: cartão pré-pago e gestão de eventos da igreja.
As decisões do projeto (domínio e técnicas) estão em [`plano-projeto-eventos.md`](plano-projeto-eventos.md).

## Requisitos

- .NET SDK 10
- PostgreSQL 14+

## Como rodar

1. Crie o banco no PostgreSQL:

   ```sql
   CREATE DATABASE agapeapp;
   ```

2. Ajuste a conexão em `src/AgapeApp.Api/appsettings.json` (ou no `appsettings.Development.json`):

   ```json
   "ConnectionStrings": {
     "Postgres": "Host=localhost;Port=5432;Database=agapeapp;Username=SEU_USUARIO;Password=SUA_SENHA"
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
  AgapeApp.Dominio          → entidades e regras de negócio
  AgapeApp.Aplicacao        → casos de uso (CQRS/MediatR), validações (FluentValidation)
  AgapeApp.Infraestrutura   → EF Core + PostgreSQL, FluentMigrator, Argon2, JWT
  AgapeApp.Api              → controladores, autenticação, Swagger
tests/
  AgapeApp.Dominio.Testes
  AgapeApp.Aplicacao.Testes
  AgapeApp.Infraestrutura.Testes
  AgapeApp.Api.TestesIntegracao
```

## Observações

- **Migrations:** ficam em `src/AgapeApp.Infraestrutura/Migracoes` e rodam na inicialização da API.
  São **somente Up** (por decisão do projeto); o `Down` lança `NotSupportedException` de propósito.
  Para reverter algo, faça manualmente no banco ou crie uma migration de correção.
- **Senhas:** hash com **Argon2id** (64 MB, 3 iterações, 4 threads).
- **Sessões:** token de acesso JWT (padrão: 30 min) + token de renovação com rotação (padrão: 7 dias).
- **Datas:** armazenadas em UTC (`timestamptz`).
