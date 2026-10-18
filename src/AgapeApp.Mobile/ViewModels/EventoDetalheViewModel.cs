using AgapeApp.Mobile.Helpers;
using AgapeApp.Mobile.Models;
using AgapeApp.Mobile.Services;
using AgapeApp.Mobile.Views.Janelas;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AgapeApp.Mobile.ViewModels;

/// <summary>
/// Detalhes do evento (administrador): situação, setores, produtos vinculados e
/// operadores designados — tudo o que o evento precisa antes de abrir para vendas.
/// As escolhas abrem janelas próprias do app (uma por vez); depois de cada gravação
/// só a seção afetada é buscada de novo.
/// </summary>
public partial class EventoDetalheViewModel : BaseViewModel
{
    private EventoAdminItem? _evento;
    private IReadOnlyList<ProdutoDTO> _catalogo = [];

    public EventoDetalheViewModel(ApiClient api, SessaoService sessao) : base(api, sessao)
    {
    }

    public ColecaoObservavel<SetorItem> Setores { get; } = [];

    public ColecaoObservavel<EventoProdutoItem> Produtos { get; } = [];

    public ColecaoObservavel<OperadorItem> Operadores { get; } = [];

    [ObservableProperty]
    private Guid eventoId;

    [ObservableProperty]
    private string eventoNome = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(
        nameof(SituacaoTexto),
        nameof(SituacaoCor),
        nameof(SituacaoFundo),
        nameof(EstaAberto),
        nameof(PodeAbrirOuEncerrar),
        nameof(TextoAbrirEncerrar),
        nameof(EstaEncerrado))]
    private string situacao = string.Empty;

    [ObservableProperty]
    private string dataTexto = string.Empty;

    public string SituacaoTexto => SituacaoEvento.Texto(Situacao);

    public Color SituacaoCor => SituacaoEvento.Cor(Situacao);

    public Color SituacaoFundo => SituacaoEvento.Fundo(Situacao);

    public bool EstaAberto => Situacao == "Aberto";

    public bool EstaEncerrado => Situacao == "Encerrado";

    public bool PodeAbrirOuEncerrar => Situacao is "Rascunho" or "Aberto";

    public string TextoAbrirEncerrar => EstaAberto ? "Encerrar evento" : "Abrir para vendas";

    public bool SemSetores => Setores.Count == 0;

    public bool SemProdutos => Produtos.Count == 0;

    public bool SemOperadores => Operadores.Count == 0;

    public string ContagemSetores => Setores.Count.ToString(Formatos.Cultura);

    public string ContagemProdutos => Produtos.Count.ToString(Formatos.Cultura);

    public string ContagemOperadores => Operadores.Count(o => o.Habilitado).ToString(Formatos.Cultura);

    public void AplicarParametros(IDictionary<string, object> parametros)
    {
        var evento = Parametros.Objeto<EventoAdminItem>(parametros, "evento");

        if (evento is null)
            return;

        if (evento.Id != EventoId)
            MarcarComoDesatualizado();

        _evento = evento;
        EventoId = evento.Id;
        EventoNome = evento.Nome;
        Situacao = evento.Situacao;
        DataTexto = evento.DataTexto;
    }

    /// <summary>Carrega na primeira vez e quando algo mudou (ex.: voltou do formulário).</summary>
    public Task IniciarAsync()
        => PrecisaRecarregar() ? ExecutarAsync(CarregarTudoAsync) : Task.CompletedTask;

    [RelayCommand]
    private Task AtualizarAsync() => ExecutarAsync(CarregarTudoAsync);

    [RelayCommand]
    private async Task EditarAsync()
    {
        if (_evento is not null)
            await NavegarAsync("evento-form", "evento", _evento);
    }

    [RelayCommand]
    private Task AbrirDashboardAsync()
        => NavegarAsync($"dashboard?eventoId={EventoId}&eventoNome={Uri.EscapeDataString(EventoNome)}");

    [RelayCommand]
    private async Task AbrirEncerrarAsync()
    {
        if (!PodeAbrirOuEncerrar)
            return;

        var abrir = !EstaAberto;

        var confirmado = abrir
            ? await ConfirmarAsync(
                "Abrir para vendas?",
                "O evento passa a aceitar recargas e vendas nos setores com operador habilitado. Confira a preparação:",
                "Abrir evento",
                TomDialogo.Sucesso,
                "\uf3c1",
                ChecagemDeAbertura())
            : await ConfirmarAsync(
                "Encerrar o evento?",
                "Depois de encerrado, o evento não aceita mais recargas nem vendas. O saldo que sobrar nos cartões fica como doação. Esta ação não pode ser desfeita.",
                "Encerrar evento",
                TomDialogo.Perigo,
                "\uf023");

        if (!confirmado)
            return;

        await ExecutarAsync(async () =>
        {
            await Api.PostSemRespostaAsync($"api/eventos/{EventoId}/{(abrir ? "abrir" : "encerrar")}", null);
            await CarregarEventoAsync();
            Notificar(abrir ? "Evento aberto para vendas" : "Evento encerrado");
        });
    }

    /// <summary>Lista do que já está pronto (ou falta) antes de abrir o evento.</summary>
    private List<ItemChecagem> ChecagemDeAbertura()
    {
        var habilitados = Operadores.Where(o => o.Habilitado).ToList();
        var setoresSemOperador = Setores
            .Where(setor => habilitados.All(o => o.SetorId != setor.Id))
            .Select(setor => setor.Nome)
            .ToList();
        var zerados = Produtos.Count(p => p.EstoqueZerado);

        var itens = new List<ItemChecagem>
        {
            new(Setores.Count == 0
                ? "Nenhum setor cadastrado"
                : Plural(Setores.Count, "setor cadastrado", "setores cadastrados"), Setores.Count > 0),
            new(Produtos.Count == 0
                ? "Nenhum produto vinculado"
                : Plural(Produtos.Count, "produto vinculado", "produtos vinculados"), Produtos.Count > 0),
            new(habilitados.Count == 0
                ? "Nenhum operador habilitado"
                : Plural(habilitados.Count, "operador habilitado", "operadores habilitados"), habilitados.Count > 0)
        };

        if (setoresSemOperador.Count > 0 && Setores.Count > 0)
            itens.Add(new ItemChecagem($"Sem operador: {string.Join(", ", setoresSemOperador)}", false));

        if (zerados > 0)
            itens.Add(new ItemChecagem(Plural(zerados, "produto com estoque zerado", "produtos com estoque zerado"), false));

        return itens;
    }

    private static string Plural(int quantidade, string singular, string plural)
        => quantidade == 1 ? $"1 {singular}" : $"{quantidade} {plural}";

    // ===== Setores =====

    [RelayCommand]
    private async Task NovoSetorAsync()
    {
        var nome = await Dialogos.PerguntarAsync(
            "Novo setor",
            "Onde o evento vai vender? Ex.: Pastel, Bebidas, Doces.",
            new OpcoesEntrada
            {
                Placeholder = "Nome do setor",
                TamanhoMaximo = 100,
                TextoConfirmar = "Criar setor",
                Icone = "\uf54e",
                Validar = texto => Setores.Any(s => string.Equals(s.Nome, texto, StringComparison.CurrentCultureIgnoreCase))
                    ? "Já existe um setor com esse nome."
                    : null
            });

        if (nome is null)
            return;

        await ExecutarAsync(async () =>
        {
            await Api.PostAsync<SetorDTO>($"api/eventos/{EventoId}/setores", new { nome });
            await CarregarSetoresAsync();
            Notificar($"Setor {nome} criado");
        });
    }

    [RelayCommand]
    private async Task AcoesSetorAsync(SetorItem? setor)
    {
        if (setor is null)
            return;

        var escolha = await EscolherAsync(
            setor.Nome,
            setor.Resumo,
            new OpcaoDialogo("produtos", "Produtos deste setor", "Marque o que este setor vende", "\uf02b", TomDialogo.Sucesso),
            new OpcaoDialogo("operador", "Designar operador aqui", "Escolha quem vai trabalhar neste setor", "\uf234"),
            new OpcaoDialogo("renomear", "Renomear setor", null, "\uf303", TomDialogo.Neutro));

        switch (escolha)
        {
            case "produtos":
                await EscolherProdutosDoSetorAsync(setor);
                break;

            case "operador":
                await DesignarOperadoresAsync(setor);
                break;

            case "renomear":
                await RenomearSetorAsync(setor);
                break;
        }
    }

    private async Task RenomearSetorAsync(SetorItem setor)
    {
        var nome = await Dialogos.PerguntarAsync(
            "Renomear setor",
            null,
            new OpcoesEntrada
            {
                ValorInicial = setor.Nome,
                Placeholder = "Nome do setor",
                TamanhoMaximo = 100,
                Icone = "\uf303"
            });

        if (nome is null || nome == setor.Nome)
            return;

        await ExecutarAsync(async () =>
        {
            await Api.PutAsync<SetorDTO>($"api/setores/{setor.Id}", new { nome });
            await Task.WhenAll(CarregarSetoresAsync(), CarregarOperadoresAsync());
            Notificar("Setor renomeado");
        });
    }

    /// <summary>Marca de uma vez todos os produtos que o setor vende e grava só o que mudou.</summary>
    private async Task EscolherProdutosDoSetorAsync(SetorItem setor)
    {
        if (Produtos.Count == 0)
        {
            await AvisarAsync(
                "Nenhum produto no evento",
                "Vincule produtos ao evento (botão Vincular, em Produtos do evento) antes de definir o que cada setor vende.",
                TomDialogo.Destaque,
                "\uf02b");
            return;
        }

        var atuais = await ExecutarAsync(() => Api.GetAsync<List<EventoProdutoDTO>>($"api/setores/{setor.Id}/produtos"));

        if (atuais is null)
            return;

        var vinculados = atuais.Select(vinculo => vinculo.Id).ToHashSet();
        var produtos = Produtos.ToList();

        var escolhidos = await Dialogos.SelecionarVariosAsync(
            $"Produtos de {setor.Nome}",
            "Marque o que este setor vende. O preço é o mesmo em todos os setores do evento.",
            produtos,
            produto => new ItemSelecao(produto.ProdutoNome, $"{produto.TextoPreco} · {produto.TextoEstoque}", Icone: "\uf02b"),
            produto => vinculados.Contains(produto.Id));

        if (escolhidos is null)
            return;

        var novos = escolhidos.Select(produto => produto.Id).ToHashSet();
        var adicionar = novos.Except(vinculados).ToList();
        var remover = vinculados.Except(novos).Where(id => produtos.Any(p => p.Id == id)).ToList();

        if (adicionar.Count == 0 && remover.Count == 0)
            return;

        await ExecutarAsync(async () =>
        {
            foreach (var id in adicionar)
                await Api.PostAsync<SetorProdutoDTO>($"api/setores/{setor.Id}/produtos", new { eventoProdutoId = id });

            foreach (var id in remover)
                await Api.DeleteAsync<object>($"api/setores/{setor.Id}/produtos/{id}");

            Notificar($"{setor.Nome}: {Plural(novos.Count, "produto", "produtos")} à venda");
        });
    }

    // ===== Produtos do evento =====

    [RelayCommand]
    private async Task VincularProdutoAsync()
    {
        var disponiveis = _catalogo
            .Where(produto => produto.Ativo && Produtos.All(item => item.ProdutoId != produto.Id))
            .OrderBy(produto => produto.Nome, StringComparer.CurrentCulture)
            .ToList();

        if (disponiveis.Count == 0)
        {
            await AvisarAsync(
                "Nada para vincular",
                "Todos os produtos ativos do catálogo já estão neste evento. Cadastre novos em Gestão › Produtos.",
                TomDialogo.Neutro,
                "\uf02b");
            return;
        }

        var produtoEscolhido = await Dialogos.SelecionarUmAsync(
            "Vincular produto",
            "Escolha um item do catálogo; em seguida você define o preço e o estoque neste evento.",
            disponiveis,
            produto => new ItemSelecao(
                produto.Nome,
                produto.PrecoPadrao is decimal preco ? $"Preço padrão {Formatos.Moeda(preco)}" : "Sem preço padrão",
                Icone: "\uf02b"));

        if (produtoEscolhido is null)
            return;

        await NavegarAsync("evento-produto-form", new Dictionary<string, object>
        {
            ["eventoId"] = EventoId,
            ["eventoNome"] = EventoNome,
            ["produto"] = produtoEscolhido
        });
    }

    [RelayCommand]
    private async Task AcoesProdutoAsync(EventoProdutoItem? produto)
    {
        if (produto is null)
            return;

        var opcoes = new List<OpcaoDialogo>
        {
            new("editar", "Editar preço e estoque", "Preço praticado e controle de estoque", "\uf303")
        };

        if (produto.ControlaEstoque)
        {
            opcoes.Add(new OpcaoDialogo("entrada", "Entrada de estoque", "Chegou mais mercadoria", "\uf067", TomDialogo.Sucesso));
            opcoes.Add(new OpcaoDialogo("baixa", "Baixa de estoque", "Perda, quebra ou retirada", "\uf068", TomDialogo.Destaque));
        }

        var escolha = await EscolherAsync(
            produto.ProdutoNome,
            $"{produto.TextoPreco} · {produto.TextoEstoque}",
            [.. opcoes]);

        switch (escolha)
        {
            case "editar":
                await NavegarAsync("evento-produto-form", new Dictionary<string, object>
                {
                    ["eventoId"] = EventoId,
                    ["eventoNome"] = EventoNome,
                    ["eventoProduto"] = produto
                });
                break;

            case "entrada":
                await AjustarEstoqueAsync(produto, entrada: true);
                break;

            case "baixa":
                await AjustarEstoqueAsync(produto, entrada: false);
                break;
        }
    }

    private async Task AjustarEstoqueAsync(EventoProdutoItem produto, bool entrada)
    {
        var digitado = await Dialogos.PerguntarAsync(
            entrada ? "Entrada de estoque" : "Baixa de estoque",
            $"{(entrada ? "Quantas unidades chegaram" : "Quantas unidades saíram")} de {produto.ProdutoNome}? Estoque atual: {produto.QuantidadeEstoque}.",
            new OpcoesEntrada
            {
                Placeholder = "Quantidade",
                Teclado = Keyboard.Numeric,
                TamanhoMaximo = 6,
                TextoConfirmar = entrada ? "Dar entrada" : "Dar baixa",
                Icone = entrada ? "\uf067" : "\uf068",
                Tom = entrada ? TomDialogo.Sucesso : TomDialogo.Destaque,
                Validar = texto =>
                {
                    if (!int.TryParse(texto, out var valor) || valor <= 0)
                        return "Informe uma quantidade maior que zero.";

                    return !entrada && valor > produto.QuantidadeEstoque
                        ? $"O estoque atual é {produto.QuantidadeEstoque}; a baixa não pode ser maior."
                        : null;
                }
            });

        if (digitado is null || !int.TryParse(digitado, out var quantidade))
            return;

        await ExecutarAsync(async () =>
        {
            await Api.PostAsync<EventoProdutoDTO>(
                $"api/evento-produtos/{produto.Id}/ajuste-estoque",
                new { quantidade = entrada ? quantidade : -quantidade, observacao = (string?)null });

            await CarregarProdutosAsync();
            Notificar($"Estoque de {produto.ProdutoNome} atualizado");
        });
    }

    // ===== Operadores =====

    [RelayCommand]
    private Task DesignarOperadorAsync() => DesignarOperadoresAsync(null);

    /// <summary>
    /// Designa um ou vários operadores de uma vez: primeiro a pessoa (com busca), depois
    /// o setor — cada passo numa janela própria, sem caixas nativas encadeadas.
    /// </summary>
    private async Task DesignarOperadoresAsync(SetorItem? setorSugerido)
    {
        if (Setores.Count == 0)
        {
            await AvisarAsync(
                "Crie um setor primeiro",
                "O operador trabalha em um setor do evento (ex.: Pastel, Bebidas). Cadastre pelo menos um setor.",
                TomDialogo.Destaque,
                "\uf54e");
            return;
        }

        var usuarios = await ExecutarAsync(() => Api.GetAsync<List<UsuarioDTO>>("api/usuarios"));

        if (usuarios is null)
            return;

        var designados = Operadores.Select(operador => operador.UsuarioId).ToHashSet();

        var disponiveis = usuarios
            .Where(usuario => usuario.Ativo && usuario.Perfil == "Operador" && !designados.Contains(usuario.Id))
            .OrderBy(usuario => usuario.Nome, StringComparer.CurrentCulture)
            .ToList();

        if (disponiveis.Count == 0)
        {
            await AvisarAsync(
                "Sem operadores disponíveis",
                "Todos os operadores ativos já estão neste evento. Para incluir mais gente, cadastre um operador em Gestão › Usuários.",
                TomDialogo.Neutro,
                "\uf0c0");
            return;
        }

        var escolhidos = await Dialogos.SelecionarVariosAsync(
            "Designar operadores",
            setorSugerido is null
                ? "Marque quem vai trabalhar no evento; depois escolha o setor."
                : $"Marque quem vai trabalhar em {setorSugerido.Nome}.",
            disponiveis,
            usuario => new ItemSelecao(usuario.Nome, usuario.Email, Formatos.Iniciais(usuario.Nome)),
            textoConfirmar: "Continuar");

        if (escolhidos is not { Count: > 0 })
            return;

        var setor = setorSugerido ?? await EscolherSetorAsync(
            escolhidos.Count == 1 ? $"Setor de {escolhidos[0].Nome}" : $"Setor dos {escolhidos.Count} operadores");

        if (setor is null)
            return;

        await ExecutarAsync(async () =>
        {
            foreach (var usuario in escolhidos)
            {
                await Api.PostAsync<EventoOperadorDTO>(
                    $"api/eventos/{EventoId}/operadores",
                    new { usuarioId = usuario.Id, setorId = setor.Id });
            }

            await CarregarOperadoresAsync();

            Notificar(escolhidos.Count == 1
                ? $"{escolhidos[0].Nome} designado em {setor.Nome}"
                : $"{escolhidos.Count} operadores designados em {setor.Nome}");
        });
    }

    [RelayCommand]
    private async Task AcoesOperadorAsync(OperadorItem? operador)
    {
        if (operador is null)
            return;

        var escolha = await EscolherAsync(
            operador.UsuarioNome,
            $"Setor {operador.SetorNome} · {operador.TextoSituacao}",
            new OpcaoDialogo("setor", "Trocar de setor", "Muda onde a pessoa opera neste evento", "\uf362"),
            operador.Habilitado
                ? new OpcaoDialogo("alternar", "Bloquear no evento", "A pessoa deixa de operar até ser liberada", "\uf05e", TomDialogo.Perigo)
                : new OpcaoDialogo("alternar", "Liberar para operar", "A pessoa volta a vender e recarregar", "\uf00c", TomDialogo.Sucesso));

        switch (escolha)
        {
            case "setor":
                var setor = await EscolherSetorAsync($"Novo setor de {operador.UsuarioNome}", operador.SetorId);

                if (setor is null || setor.Id == operador.SetorId)
                    return;

                await AtualizarDesignacaoAsync(operador, setor.Id, operador.Habilitado, $"{operador.UsuarioNome} agora opera em {setor.Nome}");
                break;

            case "alternar":
                if (operador.Habilitado && !await ConfirmarAsync(
                        "Bloquear operador?",
                        $"{operador.UsuarioNome} não conseguirá vender nem recarregar neste evento até ser liberado de novo.",
                        "Bloquear",
                        TomDialogo.Perigo,
                        "\uf05e"))
                {
                    return;
                }

                await AtualizarDesignacaoAsync(
                    operador,
                    operador.SetorId,
                    !operador.Habilitado,
                    operador.Habilitado ? $"{operador.UsuarioNome} bloqueado" : $"{operador.UsuarioNome} liberado");
                break;
        }
    }

    private Task AtualizarDesignacaoAsync(OperadorItem operador, Guid setorId, bool habilitado, string aviso)
        => ExecutarAsync(async () =>
        {
            await Api.PutAsync<EventoOperadorDTO>(
                $"api/evento-operadores/{operador.Id}",
                new { setorId, habilitado });

            await CarregarOperadoresAsync();
            Notificar(aviso);
        });

    private async Task<SetorItem?> EscolherSetorAsync(string titulo, Guid? atual = null)
    {
        if (Setores.Count == 0)
            return null;

        if (Setores.Count == 1)
            return Setores[0];

        var escolhido = await EscolherAsync(
            titulo,
            "Toque no setor.",
            Setores
                .Select(setor => new OpcaoDialogo(
                    setor.Id.ToString(),
                    setor.Nome,
                    setor.Id == atual ? "Setor atual" : setor.Resumo,
                    "\uf54e",
                    setor.Id == atual ? TomDialogo.Neutro : TomDialogo.Destaque))
                .ToArray());

        return escolhido is null ? null : Setores.FirstOrDefault(setor => setor.Id.ToString() == escolhido);
    }

    // ===== Carregamento =====

    private async Task CarregarTudoAsync()
    {
        var catalogoTask = Api.GetAsync<List<ProdutoDTO>>("api/produtos");

        await Task.WhenAll(
            CarregarEventoAsync(),
            CarregarSetoresAsync(),
            CarregarProdutosAsync(),
            CarregarOperadoresAsync(),
            catalogoTask);

        _catalogo = catalogoTask.Result;
        MarcarComoAtualizado();
    }

    private async Task CarregarEventoAsync()
    {
        var evento = await Api.GetAsync<EventoDTO>($"api/eventos/{EventoId}");

        _evento = EventoAdminItem.De(evento);
        EventoNome = _evento.Nome;
        Situacao = _evento.Situacao;
        DataTexto = _evento.DataTexto;
        MarcarComoAtualizado();
    }

    private async Task CarregarSetoresAsync()
    {
        var setores = await Api.GetAsync<List<SetorDTO>>($"api/eventos/{EventoId}/setores");

        Setores.Substituir(setores
            .OrderBy(item => item.Nome, StringComparer.CurrentCulture)
            .Select(SetorItem.De));

        AtualizarResumoDosSetores();
        OnPropertyChanged(nameof(SemSetores));
        OnPropertyChanged(nameof(ContagemSetores));
        MarcarComoAtualizado();
    }

    private async Task CarregarProdutosAsync()
    {
        var produtos = await Api.GetAsync<List<EventoProdutoDTO>>($"api/eventos/{EventoId}/produtos");

        Produtos.Substituir(produtos
            .OrderBy(item => item.ProdutoNome, StringComparer.CurrentCulture)
            .Select(EventoProdutoItem.De));

        OnPropertyChanged(nameof(SemProdutos));
        OnPropertyChanged(nameof(ContagemProdutos));
        MarcarComoAtualizado();
    }

    private async Task CarregarOperadoresAsync()
    {
        var operadores = await Api.GetAsync<List<EventoOperadorDTO>>($"api/eventos/{EventoId}/operadores");

        Operadores.Substituir(operadores
            .OrderBy(item => item.SetorNome, StringComparer.CurrentCulture)
            .ThenBy(item => item.UsuarioNome, StringComparer.CurrentCulture)
            .Select(OperadorItem.De));

        AtualizarResumoDosSetores();
        OnPropertyChanged(nameof(SemOperadores));
        OnPropertyChanged(nameof(ContagemOperadores));
        MarcarComoAtualizado();
    }

    /// <summary>Mostra em cada setor quantos operadores estão liberados nele.</summary>
    private void AtualizarResumoDosSetores()
    {
        foreach (var setor in Setores)
        {
            var quantidade = Operadores.Count(o => o.SetorId == setor.Id && o.Habilitado);
            setor.Resumo = quantidade switch
            {
                0 => "Sem operador liberado",
                1 => "1 operador",
                _ => $"{quantidade} operadores"
            };
            setor.SemOperador = quantidade == 0;
        }
    }
}
