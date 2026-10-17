using AgapeApp.Mobile.Config;
using Microsoft.AspNetCore.SignalR.Client;

namespace AgapeApp.Mobile.Services;

/// <summary>
/// Conexão em tempo real (SignalR) com o hub do dashboard do administrador.
/// O servidor avisa "operacao" sempre que uma recarga ou venda entra no evento.
/// </summary>
public class DashboardService : IAsyncDisposable
{
    private readonly SessaoService _sessao;
    private HubConnection? _conexao;
    private Guid? _eventoAtual;

    public DashboardService(SessaoService sessao) => _sessao = sessao;

    /// <summary>Disparado quando o servidor avisa que houve uma operação no evento.</summary>
    public event EventHandler? OperacaoRecebida;

    public bool Conectado => _conexao?.State == HubConnectionState.Connected;

    /// <summary>Conecta ao hub (se preciso) e passa a acompanhar o evento informado.</summary>
    public async Task EntrarNoEventoAsync(Guid eventoId, CancellationToken ct = default)
    {
        if (_conexao is null)
        {
            _conexao = new HubConnectionBuilder()
                .WithUrl(AppConfig.Url("hubs/dashboard"), opcoes =>
                {
                    // No WebSocket o servidor lê o token da query string (?access_token=...).
                    opcoes.AccessTokenProvider = async () => await _sessao.ObterTokenValidoAsync() ?? string.Empty;
                })
                .WithAutomaticReconnect()
                .Build();

            _conexao.On<AvisoOperacao>("operacao", _ =>
                OperacaoRecebida?.Invoke(this, EventArgs.Empty));

            // Depois de uma queda, os grupos são perdidos: entra de novo no evento atual.
            _conexao.Reconnected += async _ =>
            {
                if (_eventoAtual is Guid evento && _conexao is not null)
                    await _conexao.InvokeAsync("EntrarNoEvento", evento);
            };
        }

        if (_conexao.State == HubConnectionState.Disconnected)
            await _conexao.StartAsync(ct);

        if (_eventoAtual is Guid anterior && anterior != eventoId)
        {
            try
            {
                await _conexao.InvokeAsync("SairDoEvento", anterior, ct);
            }
            catch (Exception)
            {
                // Conexão caiu no meio: o grupo é refeito no próximo evento.
            }
        }

        await _conexao.InvokeAsync("EntrarNoEvento", eventoId, ct);
        _eventoAtual = eventoId;
    }

    /// <summary>Deixa de acompanhar o evento (a conexão fica pronta para o próximo).</summary>
    public async Task SairDoEventoAsync()
    {
        if (_conexao is { State: HubConnectionState.Connected } && _eventoAtual is Guid evento)
        {
            try
            {
                await _conexao.InvokeAsync("SairDoEvento", evento);
            }
            catch (Exception)
            {
                // Sem problema: a conexão será refeita quando voltar.
            }
        }

        _eventoAtual = null;
    }

    public async ValueTask DisposeAsync()
    {
        if (_conexao is not null)
            await _conexao.DisposeAsync();

        _conexao = null;
    }

    /// <summary>Formato do aviso publicado pelo servidor (só o tipo interessa ao app).</summary>
    private sealed class AvisoOperacao
    {
        public string? Tipo { get; set; }
        public Guid EventoId { get; set; }
        public DateTimeOffset Em { get; set; }
    }
}
