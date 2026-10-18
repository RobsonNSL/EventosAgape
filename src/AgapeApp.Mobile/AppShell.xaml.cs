using AgapeApp.Mobile.Helpers;
using AgapeApp.Mobile.Services;
using AgapeApp.Mobile.Views;

namespace AgapeApp.Mobile;

public partial class AppShell : Shell
{
    private readonly SessaoService _sessao;
    private readonly BiometriaService _biometria;

    public AppShell(SessaoService sessao, BiometriaService biometria)
    {
        _sessao = sessao;
        _biometria = biometria;
        InitializeComponent();

        RegistrarRotas();

        // Por padrão a aba Gestão fica escondida; o OnNavigated a revela para administradores.
        AbaGestao.IsVisible = _sessao.EhAdministrador;

        // O app sempre abre no login (e-mail/senha ou biometria); nunca entra direto.
        // Começa já no construtor: a tela de login aguarda isto antes de oferecer a biometria.
        var abertura = _sessao.PrepararAberturaAsync(manterParaBiometria: _biometria.Ativada);
        Dispatcher.Dispatch(async () => await MostrarErroDepoisDaAberturaAsync(abertura));
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

    private static async Task MostrarErroDepoisDaAberturaAsync(Task abertura)
    {
        try
        {
            await abertura;
            await MostrarErroAnteriorAsync();
        }
        catch
        {
            // Qualquer falha aqui só significa ficar na tela de login.
        }
    }

    /// <summary>Se o app fechou por erro na última vez, mostra o que aconteceu (dá para copiar e enviar).</summary>
    private static async Task MostrarErroAnteriorAsync()
    {
        var erro = RegistroErros.LerEApagar();
        if (erro is null)
            return;

        var resumo = erro.Length > 700 ? erro[..700] + "…" : erro;

        var copiar = await Dialogos.ConfirmarAsync(
            "O app fechou por um erro",
            resumo,
            "Copiar detalhes",
            "Fechar",
            Views.Janelas.TomDialogo.Perigo,
            "\uf071");

        if (copiar)
        {
            await Clipboard.Default.SetTextAsync(erro);
            Notificacao.Mostrar("Detalhes copiados: cole na conversa com o suporte");
        }
    }
}
