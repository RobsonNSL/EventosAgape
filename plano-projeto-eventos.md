# AgapeApp — Documento de Decisões

> **Versão:** 1.13 · **Data:** 17/10/2026
>
> Documento vivo: atualizar a cada decisão nova. Nenhuma linha de código começa antes deste documento estar fechado.
> **Status:** domínio fechado · técnico fechado · **nome definido: AgapeApp** · estrutura + autenticação + migrations **concluídas e validadas localmente (SQL Server — banco `dbAgape`)** · fases 1–4 da API e **fase 5 do app (operador + admin) implementadas**, com a rodada de **desempenho e telas próprias** (v1.12) — falta validar no aparelho físico (com o APK **Release**) e publicar.

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
| Banco de dados | SQL Server local: banco `dbAgape` (nuvem: a definir) |
| Arquitetura | DDD + Clean Architecture, padrão sênior .NET |
| Casos de uso | CQRS com MediatR |
| Validações | **FluentValidation** em todas as entradas; mensagens de erro centralizadas em arquivo de resources (`MensagensErro.resx`) |
| ORM | Entity Framework Core |
| Migrations | **FluentMigrator**, executadas pela API, **uma por alteração**, **somente Up (forward-only)** — rollback manual quando necessário (sob demanda) |
| Nomes | Estrutura/pastas e termos técnicos em **inglês** (`Domain`, `Controllers`, `UsuarioDTO`, `LoginCommand`); **domínio e métodos em português** (`Usuario`, `Criar`, `GerarTokenAcesso`); banco em português (`usuarios`, `criado_em`) |
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
- **No app (v1.12):** timeout de 15 s; leituras (GET) tentam 2 vezes e recarga/venda até 3 vezes com o **mesmo** `idOperacaoCliente` (`ApiClient.PostIdempotenteAsync`).

### 3.2 App mobile — padrões de interface e desempenho (v1.12)

- **Sem caixas nativas** (`DisplayAlert`, `DisplayActionSheet`, `DisplayPromptAsync`): toda confirmação, lista de ações, campo de texto ou seleção abre uma **janela própria** (`Services/Dialogos.cs` + `Views/Janelas/`) — folha que sobe da parte de baixo, no design system do app. As janelas entram numa **fila** (uma por vez), o que acabou com o travamento de encadear caixas nativas no Android.
- **Confirmações rápidas** de sucesso viram aviso que some sozinho (`Helpers/Notificacao.cs`, toast nativo no Android) — sem obrigar o toque em "OK".
- **Recarregar só quando precisa:** as telas guardam a versão dos dados (`ApiClient.VersaoDados`, que sobe a cada gravação) e só buscam de novo se algo mudou ou a tela ficou velha (2 min; lista de eventos: 1 min). Fechar uma janela não recarrega a tela de trás.
- **Listas trocadas de uma vez** (`ColecaoObservavel.Substituir`): uma notificação por carga, em vez de uma por item.
- **Bindings compilados** (`x:DataType`) em todas as telas; comandos dos itens ligados direto ao view model.
- **Pilha de navegação enxuta no terminal:** venda/recarga concluída **substitui** a tela anterior (`../venda-concluida`) e "próximo cliente" **volta** para a leitura já aberta (`Helpers/Navegacao.cs`) — antes cada cliente empilhava 3 telas.
- **Área de toque inteira** nos botões de ícone (`Controls/BotaoIcone`, `BotaoVoltar`) e cards sem sombra (a sombra pesava a rolagem).
- **APK de uso real = Release** (AOT completo, sem JIT/interpretador). O Debug é só para desenvolvimento e é bem mais lento no aparelho.

### 3.3 Convenções de nomes

- **C#:** PascalCase — domínio em português + sufixos técnicos em inglês: `Usuario`, `Cartao`, `UsuarioController`, `UsuarioDTO`, `RealizarVendaCommand`, `EfetuarRecargaCommand`; **métodos em português** (`Criar`, `CalcularHashTokenRenovacao`).
- **Banco (SQL Server):** snake_case plural em português — `usuarios`, `cartoes`, `evento_produtos`.
- **Rotas da API:** em português (ex.: `/api/vendas`, `/api/recargas`).
- **Identificadores:** `Guid` (uuid) em todas as entidades.
- **Datas/horas:** armazenadas em UTC (`datetimeoffset`), exibidas no fuso local.

---

## 4. Modelo de dados (proposta inicial)

Colunas comuns: `id`, `criado_em`, `atualizado_em`. Cadastros com **exclusão lógica** (`ativo`) — nada é apagado de verdade.

**Cadastros**
- `usuarios` — nome, email (único), senha_hash (Argon2id), perfil (`Administrador` | `Operador`), ativo, caminho_foto (opcional, foto de perfil em `wwwroot/imagens`).
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
  AgapeApp.Domain           → entidades, value objects, regras de negócio
  AgapeApp.Application      → casos de uso (CQRS), validações (FluentValidation), DTOs, interfaces
  AgapeApp.Infrastructure   → EF Core, SQL Server, FluentMigrator, Argon2, JWT, export Excel
  AgapeApp.Api              → endpoints, autenticação/autorização, SignalR, Swagger, runner de migrations
  AgapeApp.Mobile           → app MAUI (MVVM), cache SQLite, leitura de QR (fase futura)
tests/
  AgapeApp.Domain.Tests
  AgapeApp.Application.Tests
  AgapeApp.Infrastructure.Tests
  AgapeApp.Api.IntegrationTests
```

Fluxo de dependência: `Api → Application → Domain`; `Infrastructure` e `Mobile` dependem de contratos (interfaces) definidos no `Domain`/`Application`.

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
5. ~~Plano de implementação faseado~~ → **fases 2–4 concluídas na API** ✅, **fase 2 fechada com usuários completos** ✅ (editar, ativar/desativar, redefinir/trocar senha) — cadastros base, operações (recarga e venda idempotentes com saldo/estoque), relatórios com exportação Excel e notificações em tempo real (SignalR). **Fase 5 (app MAUI) implementada** ✅ — fluxo do operador completo, telas do administrador + **aba Gestão** (usuários, produtos, eventos e cartões; detalhes do evento com setores, produtos e operadores) e **foto de perfil**; falta validar no aparelho físico e publicar.
6. **Hospedagem**: definir provedor de nuvem (API + banco de dados).

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
| 1.3 | 17/10/2026 | Banco trocado para **SQL Server** local (`dbAgape`, instância `MSSQLSERVER01`); chave JWT passou a ser derivada por SHA-256 (funciona com qualquer tamanho de segredo). |
| 1.4 | 17/10/2026 | Convenção de nomes ajustada: estrutura/pastas e sufixos técnicos em **inglês** (`Domain`, `Controllers`, `UsuarioDTO`, `LoginCommand`); domínio e métodos em **português**. |
| 1.5 | 17/10/2026 | Mensagens de erro devolvidas ao cliente centralizadas em `MensagensErro.resx` (projeto Domain, acesso tipado). Campos ausentes e falhas de JSON/binding também respondem em português (ProblemDetails padronizado). |
| 1.6 | 17/10/2026 | Swagger abre automaticamente no navegador em desenvolvimento (chave `Swagger:AbrirNavegador`) e a raiz redireciona para `/swagger`; respostas 401/403 do middleware em ProblemDetails/português com o motivo (token não informado, expirado ou inválido). |
| 1.7 | 17/10/2026 | **Fases 2–4 da API concluídas**: cadastros (produtos, cartões, eventos, setores, produtos×evento/setor, operadores), operações idempotentes (recarga e venda com saldo por evento, baixa de estoque e auditoria), relatórios (resumo por setor/operador/produto, saldo em circulação, auditoria paginada por evento) e exportação Excel (ClosedXML). Migrations 0003–0016 (destaques: 0014 `vendas`, 0015 `itens_venda`, 0016 FK `movimentos_estoque.venda_id`). `logs_auditoria` ganhou a coluna `evento_id` (adição deliberada ao modelo inicial, para relatórios por evento). Notificações em tempo real no dashboard do admin via SignalR (`/hubs/dashboard`, evento `operacao`). Mensagens de erro agora somam 78 chaves no `MensagensErro.resx`. |
| 1.8 | 17/10/2026 | **Fase 2 fechada — usuários completos**: editar (nome, e-mail e perfil, com e-mail único também na edição), ativar/desativar e senha (redefinir pelo admin em `POST /api/usuarios/{id}/redefinir-senha` + trocar a própria em `POST /api/autenticacao/alterar-senha` exigindo a senha atual). Proteções de regra: não desativar a si mesmo, nunca deixar o sistema sem administrador ativo (desativar ou rebaixar o último admin é bloqueado). Desativar e redefinir/trocar senha revogam as sessões de renovação do usuário (o token de acesso atual segue valendo até expirar). Mensagens de erro: **80 chaves** no `MensagensErro.resx` (`SenhaAtualIncorreta`, `NaoPodeRemoverUltimoAdministrador`). Sem migration nova (nenhum campo novo). Smoke das fases 2–4 estendido com o fluxo de usuários — 100% verde. |
| 1.9 | 17/10/2026 | **Início da Fase 5 — app mobile**: projeto **AgapeApp.Mobile** (.NET MAUI, MVVM com `CommunityToolkit.Mvvm`) em `src/AgapeApp.Mobile` — um código para Android e iPhone, **fora da `AgapeApp.slnx`** (build próprio; o toolchain mobile não entra no build/teste da API). Design gerado no **Google Stitch** ("Agape Direct", Material 3), export salvo em `docs/design/stitch/` e aplicado como design system (primária `#2563EB`, dinheiro `#16A34A`, recarga `#F59E0B`, fonte **Inter**, botões 52px/raio 12, cards raio 16). Telas: login, meus eventos (busca + filtros), operação do evento, leitura do código do cartão, recarga (valores rápidos + valor livre), confirmações, venda com catálogo do setor, saldo, perfil e troca de senha. Sessão no SecureStorage com renovação automática de token; erros exibidos em português (ProblemDetails da API); idempotência preservada nos retries (mesmo `idOperacaoCliente`). APK de debug gerado com **0 avisos**. Próximas etapas: QR pela câmera (ZXing), biometria e telas do admin. |
| 1.10 | 17/10/2026 | **Fase 5 avançada — câmera, admin, biometria e cache**: leitura do cartão pela **câmera** com `ZXing.Net.Maui.Controls` (QR + código de barras, lanterna, confirmação automática, digitação manual de reserva; o leitor é criado/descartado a cada entrada na tela para a câmera parar de verdade); **telas do administrador** no app — dashboard do evento com totais + **SignalR ao vivo** (`Microsoft.AspNetCore.SignalR.Client`, hub `/hubs/dashboard`), saldos em circulação e auditoria paginada (consomem os relatórios da Fase 4); **entrada por biometria** com `Plugin.Maui.Biometric` (oferecida no 1º login, chave no perfil, desligada ao sair; usa o token de renovação do SecureStorage); **cache SQLite de leitura** com `sqlite-net-pcl` (eventos designados, produtos do setor e últimas operações do aparelho; fallback com aviso quando a internet cai — a escrita segue exigindo internet). `minSdk` do Android subiu de 21 para **26** (exigência do AndroidX usado pela biometria). Lista de eventos do admin alterna entre "meus" e "todos" com atalho para o dashboard. Build Android **0 avisos** · APK ~19,9 MB. |
| 1.13 | 17/10/2026 | **Crashes do app corrigidos (causa confirmada no logcat do aparelho).** (1) `StackOverflowException` ao designar operador e ao cadastrar cartão: em `BaseViewModel.ExecutarAsync<T>`, a lambda `async () => resultado = await operacao()` devolvia o valor atribuído e o C# escolhia a própria sobrecarga genérica — recursão infinita (era o "trava ao vincular operador" original). (2) `ObjectDisposedException: IServiceProvider` ao trocar de aba depois de reabrir o app: o `AppShell` era Singleton e era reaproveitado quando o Android recriava a Activity; passou a Transient. Também: registro do último erro fatal (`Helpers/RegistroErros.cs`, mostrado na próxima abertura com "Copiar detalhes"), janelas que nunca deixam exceção escapar e **cadastro de cartões pelo QR** em modo contínuo (`LeitorCartoesDialogo`: cada leitura já cadastra; duplicados avisados). Validado no aparelho: designar operador e cadastrar cartão. |
| 1.12 | 17/10/2026 | **App: desempenho, telas próprias e correção do travamento ao designar operador.** Causa do travamento: a tela de detalhes do evento encadeava 2–3 caixas nativas (`DisplayActionSheet`/`DisplayAlert`) seguidas, com a `RefreshView` disparando junto — no Android isso deixava a tela sem resposta (e as caixas apareciam como "tela preta"). Agora todas as confirmações, listas de ações, campos e seleções são **janelas próprias** em fila (`Services/Dialogos.cs`, `Views/Janelas/`), com busca nas listas grandes. **Designar operadores** marca várias pessoas de uma vez (também direto do setor); **produtos do setor** em uma lista com marcação e um único "Salvar"; **abrir o evento** mostra uma checagem (setores, produtos, operadores liberados, setores sem operador, estoque zerado); **encerrar/bloquear/desativar** pedem confirmação em vermelho. **Cartões** em sequência com o próximo código sugerido (CARD-0009 → CARD-0010). **Venda**: toque no nome soma 1, botões +/− de 44px, "sobra/falta no cartão" antes de enviar e botão bloqueado com saldo insuficiente, "Limpar". **Recarga**: valor livre digitado na própria tela, valores rápidos R$ 5/10/20/30/50/100 em grade e **confirmação do valor antes de aplicar** com saldo atual/novo (regra 2.3). **Perfil**: foto pela galeria **ou câmera**, já reduzida para 720 px/JPEG 85% no próprio seletor, sair com confirmação. **Desempenho**: pilha do terminal enxuta (venda/recarga concluída substitui a tela e "próximo cliente" volta à leitura existente — antes crescia 3 telas por cliente), recarga só quando os dados mudaram, foto de perfil em memória (antes era baixada a cada abertura da lista), listas com uma notificação por carga, bindings compilados em todas as telas, cards sem sombra, `RefreshView` em modo OneWay, retentativa automática idempotente e timeout de 15 s, brasão do login reduzido no build, teclado redimensiona a tela no Android. **Release do Android** com AOT completo (`AndroidEnableProfiledAot=false`) — o APK de uso nos eventos deve ser o Release. O atalho de servidor aparece no Release após 5 toques no brasão do login. Build Android **0 erros / 0 avisos**. API sem alterações. |
| 1.11 | 17/10/2026 | **Fase 5 concluída no app — gestão e foto de perfil**: o **administrador navega por todo o app** (o operador continua vendo pouco: Eventos e Perfil) e ganhou a aba **Gestão** com telas de **usuários**, **produtos**, **eventos** e **cartões** (criar, editar, ativar/desativar; as listas usam `?incluirInativos=true` para permitir reativar) e a tela de **detalhes do evento** — setores, produtos do evento (vincular/desvincular ao setor, preço e estoque com ações separadas de **entrada** e **baixa**), operadores designados e **abrir/encerrar**. O admin também pode **operar o terminal em qualquer setor de qualquer evento aberto** (regra que a API já tinha; o app passou a pedir o setor antes de abrir a operação). **Foto de perfil**: `POST`/`DELETE /api/autenticacao/minha-foto` (só o próprio usuário, sem variante admin), jpg/jpeg/png/webp até 5 MB com validação de *magic bytes*, arquivos em `src/AgapeApp.Api/wwwroot/imagens/` servidos estaticamente em `{apiBase}/imagens/{arquivo}` e coluna `caminho_foto` (**migration 0017**); no app o avatar aparece no perfil e na lista de eventos. Ícone do launcher (medalhão circular) e splash do Android 12+ definitivos, com remoção dos assets antigos. Mensagens de erro: **85 chaves**. Correções do dia: `[FromForm]` explícito em parâmetro `IFormFile` quebrava a geração do documento Swagger (500 em `/swagger/v1/swagger.json`) e o `FileResult.OpenReadStreamAsync()` do Xamarin não existe no MAUI 10 (virou `OpenReadAsync()`; o obsoleto `PickPhotoAsync` virou `PickPhotosAsync`). Build Android **0 erros / 0 avisos**. Pendente: validar as telas novas no aparelho físico e publicar. |
