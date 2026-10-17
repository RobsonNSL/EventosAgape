using System.Collections.ObjectModel;
using System.Net;
using AgapeApp.Mobile.Helpers;
using AgapeApp.Mobile.Models;
using AgapeApp.Mobile.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AgapeApp.Mobile.ViewModels;

/// <summary>
/// Lista de eventos: "meus eventos" (onde o operador está designado) e, para o
/// administrador, também todos os eventos com atalho para o dashboard.
/// </summary>
public partial class MeusEventosViewModel : BaseViewModel
{
    private readonly CacheService _cache;

    private IReadOnlyList<MeuEventoDTO> _meus = [];
    private IReadOnlyList<EventoDTO> _todos = [];
    private string _filtro = "todos";

    public MeusEventosViewModel(ApiClient api, SessaoService sessao, CacheService cache) : base(api, sessao)
    {
        _cache = cache;
    }

    public ObservableCollection<EventoCartao> Eventos { get; } = [];

    public ObservableCollection<FiltroChip> Filtros { get; } = [];

    public bool EhAdministrador => Sessao.EhAdministrador;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FonteMeusSelecionada), nameof(FonteTodosSelecionada), nameof(TituloListaVazia), nameof(TextoListaVazia))]
    private bool fonteTodos;

    public bool FonteMeusSelecionada => !FonteTodos;

    public bool FonteTodosSelecionada => FonteTodos;

    [ObservableProperty]
    private string nomeUsuario = string.Empty;

    [ObservableProperty]
    private string iniciais = string.Empty;

    [ObservableProperty]
    private string perfilUsuario = string.Empty;

    [ObservableProperty]
    private string busca = string.Empty;

    [ObservableProperty]
    private bool modoOffline;

    public string TituloListaVazia => FonteTodos ? "Nenhum evento cadastrado" : "Nenhum evento por aqui";

    public string TextoListaVazia => FonteTodos
        ? "Cadastre um evento no sistema para vê-lo aqui."
        : "Quando você for designado para um evento, ele aparece nesta lista.";

    public async Task CarregarAsync()
    {
        await ExecutarAsync(async () =>
        {
            var usuario = Sessao.Usuario;
            NomeUsuario = usuario?.Nome.Split(' ')[0] ?? "voluntário";
            Iniciais = Formatos.Iniciais(usuario?.Nome);
            PerfilUsuario = usuario?.Perfil ?? string.Empty;

            try
            {
                _meus = await Api.GetAsync<List<MeuEventoDTO>>("api/eventos/meus");
                await _cache.SalvarEventosAsync(_meus);
                ModoOffline = false;
            }
            catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.ServiceUnavailable)
            {
                var salvos = await _cache.LerEventosAsync();
                if (salvos.Count == 0)
                    throw;

                _meus = salvos;
                ModoOffline = true;
            }

            if (EhAdministrador)
            {
                try
                {
                    _todos = await Api.GetAsync<List<EventoDTO>>("api/eventos");
                }
                catch (ApiException)
                {
                    // Sem os eventos gerais: a visão "meus eventos" continua funcionando.
                    _todos = [];
                }
            }

            MontarFiltros();
            AplicarFiltro();
        });
    }

    [RelayCommand]
    private Task AtualizarAsync() => CarregarAsync();

    [RelayCommand]
    private void SelecionarFiltro(FiltroChip? chip)
    {
        if (chip is null)
            return;

        _filtro = chip.Chave;
        MontarFiltros();
        AplicarFiltro();
    }

    [RelayCommand]
    private void SelecionarFonte(string? fonte) => FonteTodos = fonte == "todos";

    [RelayCommand]
    private async Task AbrirPerfilAsync()
    {
        if (Shell.Current is not null)
            await Shell.Current.GoToAsync("//principal/perfil");
    }

    [RelayCommand]
    private async Task OperarAsync(EventoCartao? cartao)
    {
        if (cartao is null || !cartao.PodeOperar || Shell.Current is null)
            return;

        await Shell.Current.GoToAsync(
            $"operacao?eventoId={cartao.EventoId}&setorId={cartao.SetorId}" +
            $"&eventoNome={Uri.EscapeDataString(cartao.Nome)}&setorNome={Uri.EscapeDataString(cartao.SetorNome)}");
    }

    [RelayCommand]
    private async Task AbrirDashboardAsync(EventoCartao? cartao)
    {
        if (cartao is null || Shell.Current is null)
            return;

        await Shell.Current.GoToAsync(
            $"dashboard?eventoId={cartao.EventoId}&eventoNome={Uri.EscapeDataString(cartao.Nome)}");
    }

    partial void OnBuscaChanged(string value) => AplicarFiltro();

    partial void OnFonteTodosChanged(bool value)
    {
        _filtro = "todos";
        MontarFiltros();
        AplicarFiltro();
    }

    private IReadOnlyList<EventoCartao> Cartoes() => FonteTodos
        ? _todos.Select(EventoCartao.DeAdmin).ToList()
        : _meus.Select(e => EventoCartao.De(e, EhAdministrador)).ToList();

    private void MontarFiltros()
    {
        var cartoes = Cartoes();

        Filtros.Clear();
        Filtros.Add(FiltroChip.De("todos", "Todos", cartoes.Count, _filtro == "todos"));
        Filtros.Add(FiltroChip.De("abertos", "Abertos", cartoes.Count(e => e.Situacao == "Aberto"), _filtro == "abertos"));
        Filtros.Add(FiltroChip.De("preparacao", "Em preparação", cartoes.Count(e => e.Situacao == "Rascunho"), _filtro == "preparacao"));
        Filtros.Add(FiltroChip.De("encerrados", "Encerrados", cartoes.Count(e => e.Situacao == "Encerrado"), _filtro == "encerrados"));
    }

    private void AplicarFiltro()
    {
        var consulta = Cartoes().AsEnumerable();

        consulta = _filtro switch
        {
            "abertos" => consulta.Where(e => e.Situacao == "Aberto"),
            "preparacao" => consulta.Where(e => e.Situacao == "Rascunho"),
            "encerrados" => consulta.Where(e => e.Situacao == "Encerrado"),
            _ => consulta
        };

        if (!string.IsNullOrWhiteSpace(Busca))
            consulta = consulta.Where(e => Formatos.Contem(e.Nome, Busca) || Formatos.Contem(e.SetorNome, Busca));

        Eventos.Clear();
        foreach (var evento in consulta)
            Eventos.Add(evento);
    }
}
