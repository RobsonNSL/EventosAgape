using AgapeApp.Mobile.Services;
using AgapeApp.Mobile.Views;

namespace AgapeApp.Mobile;

public partial class AppShell : Shell
{
    private readonly SessaoService _sessao;

    public AppShell(SessaoService sessao)
    {
        InitializeComponent();
        _sessao = sessao;

        RegistrarRotas();

        // Restaura a sessão salva (se houver) sem bloquear a abertura do app.
        Dispatcher.Dispatch(async () => await RestaurarSessaoAsync());
    }

    private static void RegistrarRotas()
    {
        Routing.RegisterRoute("operacao", typeof(OperacaoEventoPage));
        Routing.RegisterRoute("leitura", typeof(LeituraQrPage));
        Routing.RegisterRoute("recarga", typeof(RecargaPage));
        Routing.RegisterRoute("recarga-concluida", typeof(RecargaConcluidaPage));
        Routing.RegisterRoute("venda", typeof(VendaPage));
        Routing.RegisterRoute("venda-concluida", typeof(VendaConcluidaPage));
        Routing.RegisterRoute("saldo", typeof(SaldoPage));
        Routing.RegisterRoute("trocar-senha", typeof(TrocarSenhaPage));
        Routing.RegisterRoute("dashboard", typeof(DashboardPage));
        Routing.RegisterRoute("saldos", typeof(SaldosPage));
        Routing.RegisterRoute("auditoria", typeof(AuditoriaPage));
    }

    private async Task RestaurarSessaoAsync()
    {
        try
        {
            if (await _sessao.CarregarAsync())
                await GoToAsync("//principal");
        }
        catch
        {
            // Sem sessão válida: permanece na tela de login.
        }
    }
}
