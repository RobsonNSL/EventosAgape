using AgapeApp.Mobile.Services;
using AgapeApp.Mobile.Views;

namespace AgapeApp.Mobile;

public partial class AppShell : Shell
{
    private readonly SessaoService _sessao;

    public AppShell(SessaoService sessao)
    {
        _sessao = sessao;
        InitializeComponent();

        RegistrarRotas();

        // Por padrão a aba Gestão fica escondida; o OnNavigated a revela para administradores.
        AbaGestao.IsVisible = _sessao.EhAdministrador;

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

        // Gestão (administrador)
        Routing.RegisterRoute("usuarios", typeof(UsuariosPage));
        Routing.RegisterRoute("usuario-form", typeof(UsuarioFormPage));
        Routing.RegisterRoute("produtos", typeof(ProdutosPage));
        Routing.RegisterRoute("produto-form", typeof(ProdutoFormPage));
        Routing.RegisterRoute("eventos-admin", typeof(EventosAdminPage));
        Routing.RegisterRoute("evento-form", typeof(EventoFormPage));
        Routing.RegisterRoute("evento-detalhe", typeof(EventoDetalhePage));
        Routing.RegisterRoute("evento-produto-form", typeof(EventoProdutoFormPage));
        Routing.RegisterRoute("cartoes", typeof(CartoesPage));
    }

    /// <summary>A aba Gestão existe só para administradores; o operador vê Eventos e Perfil.</summary>
    protected override void OnNavigated(ShellNavigatedEventArgs args)
    {
        base.OnNavigated(args);
        AbaGestao.IsVisible = _sessao.EhAdministrador;
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
