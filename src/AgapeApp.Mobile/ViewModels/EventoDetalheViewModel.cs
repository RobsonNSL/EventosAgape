using System.Collections.ObjectModel;
using AgapeApp.Mobile.Helpers;
using AgapeApp.Mobile.Models;
using AgapeApp.Mobile.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AgapeApp.Mobile.ViewModels;

/// <summary>
/// Detalhes do evento (administrador): situação, setores, produtos vinculados e
/// operadores designados — tudo o que o evento precisa antes de abrir para vendas.
/// </summary>
public partial class EventoDetalheViewModel : BaseViewModel
{
    private EventoAdminItem? _evento;
    private IReadOnlyList<ProdutoDTO> _catalogo = [];

    public EventoDetalheViewModel(ApiClient api, SessaoService sessao) : base(api, sessao)
    {
    }

    public ObservableCollection<SetorItem> Setores { get; } = [];

    public ObservableCollection<EventoProdutoItem> Produtos { get; } = [];

    public ObservableCollection<OperadorItem> Operadores { get; } = [];

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
        nameof(TextoAbrirEncerrar))]
    private string situacao = string.Empty;

    [ObservableProperty]
    private string dataTexto = string.Empty;

    public string SituacaoTexto => SituacaoEvento.Texto(Situacao);

    public Color SituacaoCor => SituacaoEvento.Cor(Situacao);

    public Color SituacaoFundo => SituacaoEvento.Fundo(Situacao);

    public bool EstaAberto => Situacao == "Aberto";

    public bool PodeAbrirOuEncerrar => Situacao is "Rascunho" or "Aberto";

    public string TextoAbrirEncerrar => EstaAberto ? "Encerrar evento" : "Abrir para vendas";

    public bool SemSetores => Setores.Count == 0;

    public bool SemProdutos => Produtos.Count == 0;

    public bool SemOperadores => Operadores.Count == 0;

    public void AplicarParametros(IDictionary<string, object> parametros)
    {
        var evento = Parametros.Objeto<EventoAdminItem>(parametros, "evento");

        if (evento is null)
            return;

        _evento = evento;
        EventoId = evento.Id;
        EventoNome = evento.Nome;
        Situacao = evento.Situacao;
        DataTexto = evento.DataTexto;
    }

    public Task IniciarAsync() => ExecutarAsync(CarregarAsync);

    [RelayCommand]
    private Task AtualizarAsync() => IniciarAsync();

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

        var confirmado = await ConfirmarAsync(
            abrir ? "Abrir para vendas" : "Encerrar evento",
            abrir
                ? "O evento passa a aceitar recargas e vendas nos setores com operador habilitado."
                : "Depois de encerrado, o evento não aceita mais recargas nem vendas.",
            abrir ? "Abrir" : "Encerrar");

        if (!confirmado)
            return;

        await ExecutarAsync(async () =>
        {
            await Api.PostSemRespostaAsync(
                $"api/eventos/{EventoId}/{(abrir ? "abrir" : "encerrar")}",
                null);

            await CarregarAsync();
        });
    }

    // ===== Setores =====

    [RelayCommand]
    private async Task NovoSetorAsync()
    {
        if (Shell.Current is null)
            return;

        var nome = await Shell.Current.DisplayPromptAsync(
            "Novo setor",
            "Onde o evento vai vender? Ex.: Pastel, Bebidas, Doces.",
            "Criar",
            "Cancelar",
            "Nome do setor",
            maxLength: 100);

        if (string.IsNullOrWhiteSpace(nome))
            return;

        await ExecutarAsync(async () =>
        {
            await Api.PostAsync<SetorDTO>($"api/eventos/{EventoId}/setores", new { nome });
            await CarregarAsync();
        });
    }

    [RelayCommand]
    private async Task AcoesSetorAsync(SetorItem? setor)
    {
        if (setor is null)
            return;

        var escolha = await EscolherAsync(setor.Nome, "Produtos deste setor", "Renomear setor");

        switch (escolha)
        {
            case "Produtos deste setor":
                await EscolherProdutosDoSetorAsync(setor);
                break;

            case "Renomear setor":
                await RenomearSetorAsync(setor);
                break;
        }
    }

    private async Task RenomearSetorAsync(SetorItem setor)
    {
        if (Shell.Current is null)
            return;

        var nome = await Shell.Current.DisplayPromptAsync(
            "Renomear setor",
            "Novo nome do setor.",
            "Salvar",
            "Cancelar",
            "Nome do setor",
            maxLength: 100,
            initialValue: setor.Nome);

        if (string.IsNullOrWhiteSpace(nome) || nome == setor.Nome)
            return;

        await ExecutarAsync(async () =>
        {
            await Api.PutAsync<SetorDTO>($"api/setores/{setor.Id}", new { nome });
            await CarregarAsync();
        });
    }

    /// <summary>
    /// Marca e desmarca os produtos que este setor vende. A folha de opções se abre de
    /// novo a cada toque, para o administrador ver o resultado na hora.
    /// </summary>
    private async Task EscolherProdutosDoSetorAsync(SetorItem setor)
    {
        if (Produtos.Count == 0)
        {
            MensagemErro = "Vincule produtos ao evento antes de definir o que este setor vende.";
            return;
        }

        var vinculados = new HashSet<Guid>();

        await ExecutarAsync(async () =>
        {
            var atuais = await Api.GetAsync<List<EventoProdutoDTO>>($"api/setores/{setor.Id}/produtos");

            foreach (var vinculo in atuais)
                vinculados.Add(vinculo.Id);
        });

        if (TemErro)
            return;

        while (true)
        {
            var opcoes = Produtos
                .Select(produto => $"{(vinculados.Contains(produto.Id) ? "✓" : "·")} {produto.ProdutoNome}")
                .ToArray();

            var escolhido = await EscolherAsync($"Produtos de {setor.Nome}", opcoes);

            if (string.IsNullOrWhiteSpace(escolhido) || escolhido == "Cancelar")
                return;

            var indice = Array.IndexOf(opcoes, escolhido);

            if (indice < 0)
                return;

            var produto = Produtos[indice];
            var jaVinculado = vinculados.Contains(produto.Id);

            await ExecutarAsync(async () =>
            {
                if (jaVinculado)
                {
                    await Api.DeleteAsync<object>($"api/setores/{setor.Id}/produtos/{produto.Id}");
                    vinculados.Remove(produto.Id);
                }
                else
                {
                    await Api.PostAsync<SetorProdutoDTO>(
                        $"api/setores/{setor.Id}/produtos",
                        new { eventoProdutoId = produto.Id });

                    vinculados.Add(produto.Id);
                }
            });

            if (TemErro)
                return;
        }
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
            MensagemErro = "Não há produtos do catálogo para vincular. Cadastre um produto em Gestão › Produtos.";
            return;
        }

        var escolhido = await EscolherAsync(
            "Vincular produto ao evento",
            disponiveis.Select(produto => produto.Nome).ToArray());

        if (string.IsNullOrWhiteSpace(escolhido) || escolhido == "Cancelar")
            return;

        var produtoEscolhido = disponiveis.FirstOrDefault(produto => produto.Nome == escolhido);

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

        var escolha = produto.ControlaEstoque
            ? await EscolherAsync(produto.ProdutoNome, "Editar preço e estoque", "Entrada de estoque", "Baixa de estoque")
            : await EscolherAsync(produto.ProdutoNome, "Editar preço e estoque");

        switch (escolha)
        {
            case "Editar preço e estoque":
                await NavegarAsync("evento-produto-form", new Dictionary<string, object>
                {
                    ["eventoId"] = EventoId,
                    ["eventoNome"] = EventoNome,
                    ["eventoProduto"] = produto
                });
                break;

            case "Entrada de estoque":
                await AjustarEstoqueAsync(produto, entrada: true);
                break;

            case "Baixa de estoque":
                await AjustarEstoqueAsync(produto, entrada: false);
                break;
        }
    }

    private async Task AjustarEstoqueAsync(EventoProdutoItem produto, bool entrada)
    {
        if (Shell.Current is null)
            return;

        var digitado = await Shell.Current.DisplayPromptAsync(
            entrada ? "Entrada de estoque" : "Baixa de estoque",
            $"{(entrada ? "Quanto chegou" : "Quanto saiu")} de {produto.ProdutoNome}? Estoque atual: {produto.QuantidadeEstoque}.",
            "Ajustar",
            "Cancelar",
            "Quantidade",
            maxLength: 6,
            keyboard: Keyboard.Numeric);

        if (string.IsNullOrWhiteSpace(digitado))
            return;

        if (!int.TryParse(digitado, out var quantidade) || quantidade <= 0)
        {
            MensagemErro = "Informe uma quantidade maior que zero.";
            return;
        }

        var ajuste = entrada ? quantidade : -quantidade;

        await ExecutarAsync(async () =>
        {
            await Api.PostAsync<EventoProdutoDTO>(
                $"api/evento-produtos/{produto.Id}/ajuste-estoque",
                new { quantidade = ajuste, observacao = (string?)null });

            await CarregarAsync();
        });
    }

    // ===== Operadores =====

    [RelayCommand]
    private async Task DesignarOperadorAsync()
    {
        if (Setores.Count == 0)
        {
            MensagemErro = "Cadastre pelo menos um setor antes de designar um operador.";
            return;
        }

        var usuarios = await ExecutarAsync<List<UsuarioDTO>>(() => Api.GetAsync<List<UsuarioDTO>>("api/usuarios"));

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
                "Cadastre um operador em Gestão › Usuários (ou ele já está designado neste evento).");
            return;
        }

        var escolhido = await EscolherAsync(
            "Designar operador",
            disponiveis.Select(usuario => usuario.Nome).ToArray());

        if (string.IsNullOrWhiteSpace(escolhido) || escolhido == "Cancelar")
            return;

        var operador = disponiveis.FirstOrDefault(usuario => usuario.Nome == escolhido);

        if (operador is null)
            return;

        var setor = await EscolherSetorAsync("Em qual setor ele vai operar?");

        if (setor is null)
            return;

        await ExecutarAsync(async () =>
        {
            await Api.PostAsync<EventoOperadorDTO>(
                $"api/eventos/{EventoId}/operadores",
                new { usuarioId = operador.Id, setorId = setor.Id });

            await CarregarAsync();
        });
    }

    [RelayCommand]
    private async Task AcoesOperadorAsync(OperadorItem? operador)
    {
        if (operador is null)
            return;

        var escolha = await EscolherAsync(operador.UsuarioNome, operador.TextoAlternar, "Trocar de setor");

        switch (escolha)
        {
            case "Trocar de setor":
                var setor = await EscolherSetorAsync("Novo setor do operador");

                if (setor is null || setor.Id == operador.SetorId)
                    return;

                await ExecutarAsync(async () =>
                {
                    await Api.PutAsync<EventoOperadorDTO>(
                        $"api/evento-operadores/{operador.Id}",
                        new { setorId = setor.Id, habilitado = operador.Habilitado });

                    await CarregarAsync();
                });
                break;

            case "Bloquear no setor":
            case "Habilitar no setor":
                await ExecutarAsync(async () =>
                {
                    await Api.PutAsync<EventoOperadorDTO>(
                        $"api/evento-operadores/{operador.Id}",
                        new { setorId = operador.SetorId, habilitado = !operador.Habilitado });

                    await CarregarAsync();
                });
                break;
        }
    }

    private async Task<SetorItem?> EscolherSetorAsync(string titulo)
    {
        if (Setores.Count == 0)
            return null;

        if (Setores.Count == 1)
            return Setores[0];

        var escolhido = await EscolherAsync(titulo, Setores.Select(setor => setor.Nome).ToArray());

        return string.IsNullOrWhiteSpace(escolhido) || escolhido == "Cancelar"
            ? null
            : Setores.FirstOrDefault(setor => setor.Nome == escolhido);
    }

    // ===== Carregamento =====

    private async Task CarregarAsync()
    {
        var eventoTask = Api.GetAsync<EventoDTO>($"api/eventos/{EventoId}");
        var setoresTask = Api.GetAsync<List<SetorDTO>>($"api/eventos/{EventoId}/setores");
        var produtosTask = Api.GetAsync<List<EventoProdutoDTO>>($"api/eventos/{EventoId}/produtos");
        var operadoresTask = Api.GetAsync<List<EventoOperadorDTO>>($"api/eventos/{EventoId}/operadores");
        var catalogoTask = Api.GetAsync<List<ProdutoDTO>>("api/produtos");

        await Task.WhenAll(eventoTask, setoresTask, produtosTask, operadoresTask, catalogoTask);

        _evento = EventoAdminItem.De(eventoTask.Result);
        EventoNome = _evento.Nome;
        Situacao = _evento.Situacao;
        DataTexto = _evento.DataTexto;

        Setores.Clear();
        foreach (var setor in setoresTask.Result.OrderBy(item => item.Nome, StringComparer.CurrentCulture))
            Setores.Add(SetorItem.De(setor));

        Produtos.Clear();
        foreach (var produto in produtosTask.Result.OrderBy(item => item.ProdutoNome, StringComparer.CurrentCulture))
            Produtos.Add(EventoProdutoItem.De(produto));

        Operadores.Clear();
        foreach (var operador in operadoresTask.Result
                     .OrderBy(item => item.SetorNome, StringComparer.CurrentCulture)
                     .ThenBy(item => item.UsuarioNome, StringComparer.CurrentCulture))
        {
            Operadores.Add(OperadorItem.De(operador));
        }

        _catalogo = catalogoTask.Result;

        OnPropertyChanged(nameof(SemSetores));
        OnPropertyChanged(nameof(SemProdutos));
        OnPropertyChanged(nameof(SemOperadores));
    }
}
