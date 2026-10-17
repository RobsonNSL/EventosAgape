# AgapeApp — Documento de Decisões

> **Versão:** 1.2 · **Data:** 17/10/2026
>
> Documento vivo: atualizar a cada decisão nova. Nenhuma linha de código começa antes deste documento estar fechado.
> **Status:** domínio fechado · técnico fechado · **nome definido: AgapeApp** · estrutura + autenticação + migrations **concluídas (aguardando testes locais)**.

---

## 1. Visão geral

Aplicativo para a **pastoral de eventos da igreja**, baseado em **cartão pré-pago**:

- O cliente pega/compra um cartão e **recarrega crédito pagando na maquininha** (fora do app — sem integração de pagamento).
- O operador registra a recarga **apenas no banco de dados** (controle de saldo — a operação financeira acontece na maquininha).
- Nos eventos, o operador **escaneia o QR do cartão e debita** o consumo (pastel, refrigerante, camisa etc.).
- O admin gerencia usuários, eventos, produtos, preços, estoque e relatórios.

**Usuários do sistema:** apenas **Administradores** e **Operadores** (não há login de cliente final).

**Clientes do sistema:** app mobile (.NET MAUI — Android + iPhone). Futuramente, **painel web** para todas as operações do admin, consumindo a **mesma API**.

---

## 2. Regras de domínio (decididas)

### 2.1 Cartão
- Cartão **ao portador**: sem cadastro prévio de pessoa, sem vínculo com cliente.
- Identificação por **código único** (ex.: `CARD-0001`) lido via QR code.
- QR ilegível → operador pode **digitar o código manualmente**.
- Cadastro **um a um**, feito pelos admins (funcionalidade do admin).
- Cartão não cadastrado → operação **rejeitada** com mensagem clara.

### 2.2 Saldo
- Saldo **por cartão × evento** — o mesmo cartão pode ter saldos independentes em eventos diferentes; o saldo **nunca atravessa** eventos.
- **Não há transferência** entre cartões e **não há estorno** (por enquanto).
- Sobra de saldo ao fim do evento fica como **doação** (sem devolução).
- Recarga é **sempre** vinculada ao evento em que o operador está trabalhando — não existe recarga "geral".

### 2.3 Recarga
1. Cliente solicita o valor → paga na **maquininha** (fora do app).
2. Pagamento aprovado → operador digita o valor no app (**presets de R$ 5 a R$ 100 + campo livre editável, sem valor mínimo ou máximo**).
3. App mostra o valor digitado **antes de aplicar**, e o operador confirma.
4. API valida (cartão cadastrado, evento aberto, conexão) e **soma** ao saldo do evento.
5. API retorna **o saldo atual do cartão** — tenha dado certo ou não.
6. Operação **auditada** (quem, quanto, quando, qual dispositivo).

### 2.4 Venda
1. Operador **escaneia o cartão**.
2. Monta o pedido com os produtos **do seu setor** (grade de toque rápido, com quantidade).
3. Confirma → API valida **saldo e estoque** → **debita** (tudo-ou-nada; sem débito parcial).
4. API retorna **o saldo atual do cartão**, tenha dado certo ou não.
5. **Sem fiado** (saldo nunca fica negativo) e **sem estorno** (por enquanto).

### 2.5 Estoque
- Definido **por evento** e **compartilhado** entre todos os setores que vendem aquele produto.
- **Opcional por produto**: com controle ativo, a venda **bloqueia** quando o estoque zera; sem controle, nenhuma validação.
- **Ajuste manual** (admin) permitido no meio do evento (ex.: comprar mais refrigerante).
- Toda movimentação registrada (`movimentos_estoque`): venda, ajuste manual, estoque inicial.

### 2.6 Setores
- **Setor** = divisão do evento (ex.: Pastel, Bebidas, Cachorro-quente, Doces).
- Cada setor define **quais produtos vende** (pode vender 1 ou vários).
- **Preço é definido no evento** — o mesmo produto tem o mesmo preço em todos os setores do evento.

### 2.7 Operadores
- Designado a **um setor por evento**; pode ser designado a **vários eventos**.
- Um setor pode ter **vários operadores ao mesmo tempo**.
- Operador designado aparece **desabilitado** até o **admin abrir o evento**.
- Ao entrar, escolhe entre **os eventos em que está designado**.
- **1 aparelho por pessoa** (~50 operadores).
- Login: **e-mail + senha**; depois do primeiro login, **biometria** para desbloquear.

### 2.8 Eventos
- Ciclo de vida: **Rascunho → Aberto → Encerrado** (admin abre/encerra manualmente).
- Vários eventos podem rodar **ao mesmo tempo** (ex.: pastelaria + bazar no mesmo dia).
- Operador **só enxerga** os eventos em que está designado.

### 2.9 Relatórios (admin)
- **Em tempo real**: total de vendas, produto mais vendido, vendas por operador, vendas por produto, vendas **por setor**, recargas realizadas, auditoria.
- **Exportação para Excel**.

---

## 3. Decisões técnicas

| Assunto | Decisão |
| --- | --- |
| Linguagem / runtime | C# / .NET (LTS mais recente) |
| API | ASP.NET Core Web API |
| App mobile | .NET MAUI (Android + iPhone) |
| Banco de dados | PostgreSQL (nuvem) |
| Arquitetura | DDD + Clean Architecture, padrão sênior .NET |
| Casos de uso | CQRS com MediatR |
| Validações | **FluentValidation** em todas as entradas, com respostas de erro padronizadas |
| ORM | Entity Framework Core |
| Migrations | **FluentMigrator**, executadas pela API, **uma por alteração**, **somente Up (forward-only)** — rollback manual quando necessário (sob demanda) |
| Nomes | **Tudo em português**: código (classes, membros) e banco (tabelas, colunas) |
| Autenticação | JWT + refresh token; senha com **Argon2id**; biometria no app |
| Tempo real | SignalR (dashboard do admin) |
| Excel | ClosedXML |
| QR Code | Leitura por câmera no app + digitação manual de fallback |
| Conectividade | **Opção A — online obrigatório com blindagem** (detalhada em 3.1) |

### 3.1 Conectividade (decisão: Opção A)

- O app **verifica a conexão** (wifi ou dados) antes de recarga e venda.
- Escrever no banco **exige internet** — sem fila offline.
- **Blindagem contra rede ruim e duplicidade:**
  - Toda operação carrega um **ID único gerado pelo app** → a API é **idempotente**: retry, toque duplo no botão e rede lenta **nunca duplicam** uma venda ou recarga.
  - Timeout curto + retry automático com o **mesmo ID de operação**.
  - Saldo e estoque são debitados **transacionalmente** no banco (impossível vender produto que zerou nesse intervalo).
- **SQLite no celular** existe apenas como **cache de leitura** (produtos, preços, setores, últimas operações) → o app abre instantâneo mesmo com internet ruim.
- Se a internet cair de vez, as operações **param** (aceito conscientemente). Mitigação futura na infraestrutura (ex.: chip 4G reserva).

### 3.2 Convenções de nomes

- **C#:** PascalCase em português — `Usuario`, `Cartao`, `RealizarVendaCommand` → `RealizarVendaComando`, `EfetuarRecargaComando`.
- **Banco (PostgreSQL):** snake_case plural em português — `usuarios`, `cartoes`, `evento_produtos`.
- **Rotas da API:** em português (ex.: `/api/vendas`, `/api/recargas`).
- **Identificadores:** `Guid` (uuid) em todas as entidades.
- **Datas/horas:** armazenadas em UTC (`timestamptz`), exibidas no fuso local.

---

## 4. Modelo de dados (proposta inicial)

Colunas comuns: `id`, `criado_em`, `atualizado_em`. Cadastros com **exclusão lógica** (`ativo`) — nada é apagado de verdade.

**Cadastros**
- `usuarios` — nome, email (único), senha_hash (Argon2id), perfil (`Administrador` | `Operador`), ativo.
- `tokens_renovacao` — sessões / refresh tokens (hash do token, expiração, revogação, dispositivo).
- `cartoes` — codigo (único, ex. `CARD-0001`), ativo.
- `produtos` — nome, preco_padrao (opcional, para pré-preencher no evento), ativo.
- `eventos` — nome, data_inicio, data_fim, situacao (`Rascunho` | `Aberto` | `Encerrado`), criado_por.
- `setores` — evento_id, nome.
- `evento_produtos` — evento_id, produto_id, preco, controla_estoque, quantidade_estoque. Único por (evento, produto).
- `setor_produtos` — setor_id, evento_produto_id. Define o que cada setor vende.
- `evento_operadores` — evento_id, usuario_id, setor_id, habilitado. Único por (evento, usuario).

**Saldos e operações**
- `saldos_cartao_evento` — cartao_id, evento_id, saldo. Único por (cartão, evento); criado na 1ª recarga.
- `recargas` — cartao_id, evento_id, setor_id, usuario_id (operador), valor, id_operacao_cliente (idempotência), dispositivo, criado_em.
- `vendas` — cartao_id, evento_id, setor_id, usuario_id, valor_total, id_operacao_cliente (idempotência), dispositivo, criado_em.
- `itens_venda` — venda_id, evento_produto_id, nome_snapshot, preco_unitario_snapshot, quantidade, valor_linha.
- `movimentos_estoque` — evento_produto_id, quantidade (delta), motivo (`Venda` | `AjusteManual` | `EstoqueInicial`), venda_id (quando venda), usuario_id, observacao, criado_em.
- `logs_auditoria` — usuario_id, acao, entidade, entidade_id, dados (json), dispositivo, criado_em.

> **Snapshots** em `itens_venda` (nome e preço) garantem que mudanças de preço no meio do evento não alterem o histórico.
> **Idempotência** (`id_operacao_cliente` + índice único) garante que retry/rede lenta nunca duplique operações.

---

## 5. Arquitetura da solução

Um repositório, dois clientes (app hoje; web depois) sobre a mesma API.

```
AgapeApp.slnx
src/
  AgapeApp.Dominio          → entidades, value objects, regras de negócio
  AgapeApp.Aplicacao        → casos de uso (CQRS), validações (FluentValidation), DTOs, interfaces
  AgapeApp.Infraestrutura   → EF Core, PostgreSQL, FluentMigrator, Argon2, JWT, export Excel
  AgapeApp.Api              → endpoints, autenticação/autorização, SignalR, Swagger, runner de migrations
  AgapeApp.Mobile           → app MAUI (MVVM), cache SQLite, leitura de QR (fase futura)
tests/
  AgapeApp.Dominio.Testes
  AgapeApp.Aplicacao.Testes
  AgapeApp.Infraestrutura.Testes
  AgapeApp.Api.TestesIntegracao
```

Fluxo de dependência: `Api → Aplicacao → Dominio`; `Infraestrutura` e `Mobile` dependem de contratos (interfaces) definidos no `Dominio`/`Aplicacao`.

---

## 6. Nome do produto — decidido: AgapeApp ✅

- **AgapeApp** — curto, com identidade cristã (do grego *agapē*, o amor fraterno / refeição fraterna da igreja primitiva), e funciona bem como nome de app ("Ágape" na tela do celular).
- No código, sem acentos: `AgapeApp` (solução, namespaces, projetos).

*Nomes considerados:* CartaoEventos, Ágape, Partilha, PastoralEventos, Quermesse, EventosApp.

---

## 7. Próximos passos

1. ~~Escolher o nome~~ → **AgapeApp** ✅
2. ~~Estrutura do repositório + autenticação + migrations~~ → **concluído (API)** ✅ — solução DDD, login/renovação/saída, Argon2id, JWT + refresh com rotação, FluentMigrator forward-only (`usuarios`, `tokens_renovacao`) e criação do primeiro administrador na inicialização.
3. **Mapa de telas e fluxos** (operador e admin).
4. **Detalhamento dos casos de uso / endpoints** (contrato completo da API).
5. **Plano de implementação faseado** (cadastros → recarga → venda → relatórios).
6. **Hospedagem**: definir provedor de nuvem (API + PostgreSQL).

### Pontos em aberto registrados

- Confirmar fluxo de "renovar token" no app (rotação de refresh token já implementada na API).
- Confirmação das rotas da API em português no detalhamento dos endpoints.

---

## Changelog

| Versão | Data | Alterações |
| --- | --- | --- |
| 1.0 | 17/10/2026 | Criação do documento (domínio e decisões técnicas fechados). |
| 1.1 | 17/10/2026 | Nome definido: **AgapeApp**. Estrutura inicial + autenticação + migrations em andamento. |
| 1.2 | 17/10/2026 | API v1 de autenticação concluída: solução DDD, Argon2id, JWT + refresh, FluentMigrator forward-only e seed do administrador. |
