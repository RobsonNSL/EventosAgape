# AgapeApp — Checklist de progresso

> Acompanha a execução do [`plano-projeto-eventos.md`](plano-projeto-eventos.md) (decisões).
> Atualizado em: 17/10/2026
> Legenda: `[x]` concluído · `[ ]` pendente
> **Situação:** fases 1–4 implementadas na API (build sem avisos; 51 testes de domínio + 3 de aplicação + 6 de infraestrutura). Migrations 0001–0016.

---

## ✅ Fase 1 — Fundação (concluída e validada)

### Planejamento
- [x] Documento de decisões do domínio + técnicas (plano v1.6)
- [x] Nome do produto: **AgapeApp**

### Solução
- [x] Solução DDD + Clean Architecture (`AgapeApp.slnx`) — 4 projetos `src` + 4 de testes
- [x] Convenção de nomes: estrutura/técnico em inglês, domínio/métodos em português, banco em snake_case português
- [x] Pacotes: MediatR (CQRS), FluentValidation, EF Core SqlServer, FluentMigrator, Argon2, JWT

### Banco de dados (SQL Server local — `dbAgape`)
- [x] Migrations forward-only (somente Up) rodando na inicialização da API
- [x] Tabelas `usuarios` e `tokens_renovacao`
- [x] Seed do primeiro administrador

### Autenticação & segurança
- [x] Senha com **Argon2id**
- [x] Token de acesso JWT (30 min) + token de renovação (7 dias) com **rotação** e revogação
- [x] Token de renovação guardado só como hash (SHA-256)
- [x] Endpoints: `/login`, `/renovar`, `/sair`, `/eu`
- [x] Chave JWT derivada por SHA-256 (funciona com segredo de qualquer tamanho)
- [x] Perfis Administrador/Operador + política "SomenteAdministrador"

### Usuários
- [x] Criar usuário (endpoint admin)
- [x] Listar usuários (endpoint admin)
- [x] Editar (nome, e-mail e perfil) — e-mail único também na edição
- [x] Ativar/desativar (não desativa a si mesmo; nunca deixa o sistema sem administrador ativo)
- [x] Senha: redefinir pelo admin ("esqueci a senha") e trocar a própria (exige a senha atual)
- [x] Desativar/redefinir senha revogam as sessões de renovação do usuário

### Erros padronizados (ProblemDetails em português)
- [x] Tratador global de exceções (inclui saldo insuficiente com o saldo atual na resposta)
- [x] Mensagens centralizadas em `MensagensErro.resx` (80 chaves, acesso tipado + teste automático)
- [x] JSON malformado / campos ausentes / tipo errado → resposta em português
- [x] 401/403 do middleware em português, com o motivo (não informado, expirado, inválido, sem permissão)

### Swagger / experiência de desenvolvimento
- [x] Swagger com autenticação Bearer
- [x] Abre automaticamente no navegador ao iniciar (Development; chave `Swagger:AbrirNavegador`)
- [x] Raiz `http://localhost:5180/` redireciona para `/swagger`

### Qualidade
- [x] Smoke ponta a ponta da autenticação contra SQL Server real — 16 checagens
- [x] `README.md` + plano atualizados

---

## ✅ Fase 2 — Cadastros base (concluída)

- [x] Produtos (nome, preço padrão opcional, ativo) — criar/editar/ativar/desativar/listar/obter
- [x] Cartões (código único ex. `CARD-0001`, ativo; cadastro um a um)
- [x] Eventos (nome, datas, ciclo Rascunho → Aberto → Encerrado) + "meus eventos" do operador
- [x] Setores do evento (ex.: Pastel, Bebidas)
- [x] Evento × Produtos (preço no evento, controla estoque?, estoque inicial) + ajuste manual de estoque
- [x] Setor × Produtos (o que cada setor vende)
- [x] Evento × Operadores (setor designado, habilitado pelo admin)
- [x] Usuários: editar, desativar/ativar e trocar senha — **fase 2 concluída em 17/10/2026** (fluxo validado no smoke ponta a ponta)

## ✅ Fase 3 — Operações (API concluída)

- [x] Saldos cartão × evento (criado na 1ª recarga; nunca atravessa evento)
- [x] Recarga idempotente (retorna o saldo; auditada; notifica o dashboard)
- [x] Venda (produtos do setor, valida saldo **e** estoque, tudo-ou-nada, snapshots de nome/preço)
- [x] Estoque por evento + `movimentos_estoque` (venda, ajuste manual, estoque inicial)
- [x] `logs_auditoria` (quem, quando, dispositivo, evento)
- [x] Idempotência (`id_operacao_cliente` + índice único em recargas e vendas)
- [x] Consulta de saldo do cartão por evento (leitura do QR pelo operador)
- [ ] Presets de R$ 5–100 + valor livre — é do app (Fase 5); a API aceita qualquer valor > 0

## ✅ Fase 4 — Relatórios & tempo real (API concluída)

- [x] Resumo do evento (totais de recargas/vendas, saldo em circulação, por setor/operador/produto)
- [x] Saldo em circulação (cartões com saldo > 0) e auditoria por evento (paginada)
- [x] Dashboard admin com SignalR (`/hubs/dashboard`, grupo `evento-{id}`, evento client `operacao`)
- [x] Exportação para Excel com ClosedXML (abas: Resumo, Por setor, Por operador, Por produto, Saldos)

## 🚀 Fase 5 — Clientes & publicação

### App mobile (.NET MAUI)
- [ ] Login + biometria (depois do 1º login)
- [ ] Seleção entre os eventos em que o operador está designado
- [ ] Recarga e venda com leitura de QR (digitação manual como fallback)
- [ ] Cache SQLite de leitura (produtos, preços, setores, últimas operações)
- [ ] Dashboard do admin no app (consome o resumo + SignalR)

### Publicação
- [ ] Painel web do admin (futuro, mesma API)
- [ ] Deploy — IIS primeiro na igreja? depois nuvem (provedor a definir)
- [ ] Trocar a chave JWT (`secretHash`) por segredo longo e aleatório
- [ ] Git / versionamento — fora desta fase por decisão

---

## 📝 Pendências registradas (pontos em aberto)

- [ ] Contrato completo dos endpoints da API (rotas em português) — passo 4 do plano
- [ ] Mapa de telas e fluxos (operador e admin) — passo 3 do plano
- [ ] Confirmar fluxo de renovação de token no app
- [ ] Renomear variáveis locais dos testes para inglês (`validador` → `validator` etc.) — oferta em aberto
- [ ] Avaliar limpeza periódica de tokens de renovação expirados
- [x] ~~Smoke ponta a ponta das fases 2–4~~ — **concluído em 17/10/2026**: cadastros → operador → abrir evento → recarga idempotente → venda (saldo + estoque) → erros em português → relatórios → Excel. Achou e corrigiu 1 bug real (ordem de INSERT da venda no EF) e validou tudo contra o SQL Server. Estendido no mesmo dia com o **fechamento da fase 2**: editar usuário, e-mail duplicado (409), desativar a si mesmo e rebaixar o último administrador (400), desativar/reativar com login e renovação bloqueados (sessões revogadas), redefinir e trocar a própria senha com restauração dos valores originais.
- [ ] Avaliar limitador de tentativas de login (proteção contra força bruta) — não previsto no plano ainda
