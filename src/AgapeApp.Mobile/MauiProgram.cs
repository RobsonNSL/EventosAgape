using AgapeApp.Mobile.Helpers;
using AgapeApp.Mobile.Services;
using AgapeApp.Mobile.ViewModels;
using AgapeApp.Mobile.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Plugin.Maui.Biometric;
using ZXing.Net.Maui.Controls;

namespace AgapeApp.Mobile;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		// Primeiro de tudo: qualquer erro fatal fica registrado para a próxima abertura.
		RegistroErros.Iniciar();

		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.UseBarcodeReader()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("Inter-Regular.ttf", "Inter");
				fonts.AddFont("Inter-Medium.ttf", "InterMedium");
				fonts.AddFont("Inter-SemiBold.ttf", "InterSemiBold");
				fonts.AddFont("Inter-Bold.ttf", "InterBold");
				fonts.AddFont("Inter-ExtraBold.ttf", "InterExtraBold");
				fonts.AddFont("fa-solid-900.ttf", "fa");
			});

		// ===== Serviços =====
		// Tempo curto: venda e recarga tentam de novo sozinhas (mesmo id de operação) em vez de
		// deixar o operador 30s olhando para a tela quando a rede do evento oscila.
		builder.Services.AddSingleton(new HttpClient { Timeout = TimeSpan.FromSeconds(15) });
		builder.Services.AddSingleton<SessaoService>();
		builder.Services.AddSingleton<ApiClient>();
		builder.Services.AddSingleton<CacheService>();
		builder.Services.AddSingleton<FotoUsuarioService>();
		builder.Services.AddSingleton<DashboardService>();
		builder.Services.AddSingleton<IBiometric>(_ => BiometricAuthenticationService.Default);
		builder.Services.AddSingleton<BiometriaService>();
		// Transient: quando o Android recria a Activity (sair pelo voltar e reabrir, voltar da
		// câmera...), cada janela nova ganha um Shell novo. Com Singleton, as abas antigas ficavam
		// presas ao contexto descartado e trocar de aba fechava o app (ObjectDisposedException).
		builder.Services.AddTransient<AppShell>();

		// ===== ViewModels =====
		builder.Services.AddTransient<LoginViewModel>();
		builder.Services.AddTransient<MeusEventosViewModel>();
		builder.Services.AddTransient<OperacaoEventoViewModel>();
		builder.Services.AddTransient<LeituraQrViewModel>();
		builder.Services.AddTransient<RecargaViewModel>();
		builder.Services.AddTransient<RecargaConcluidaViewModel>();
		builder.Services.AddTransient<VendaViewModel>();
		builder.Services.AddTransient<VendaConcluidaViewModel>();
		builder.Services.AddTransient<SaldoViewModel>();
		builder.Services.AddTransient<PerfilViewModel>();
		builder.Services.AddTransient<TrocarSenhaViewModel>();
		builder.Services.AddTransient<DashboardViewModel>();
		builder.Services.AddTransient<SaldosViewModel>();
		builder.Services.AddTransient<AuditoriaViewModel>();
		builder.Services.AddTransient<GestaoViewModel>();
		builder.Services.AddTransient<UsuariosViewModel>();
		builder.Services.AddTransient<UsuarioFormViewModel>();
		builder.Services.AddTransient<ProdutosViewModel>();
		builder.Services.AddTransient<ProdutoFormViewModel>();
		builder.Services.AddTransient<EventosAdminViewModel>();
		builder.Services.AddTransient<EventoFormViewModel>();
		builder.Services.AddTransient<EventoDetalheViewModel>();
		builder.Services.AddTransient<EventoProdutoFormViewModel>();
		builder.Services.AddTransient<CartoesViewModel>();

		// ===== Páginas =====
		builder.Services.AddTransient<LoginPage>();
		builder.Services.AddTransient<MeusEventosPage>();
		builder.Services.AddTransient<OperacaoEventoPage>();
		builder.Services.AddTransient<LeituraQrPage>();
		builder.Services.AddTransient<RecargaPage>();
		builder.Services.AddTransient<RecargaConcluidaPage>();
		builder.Services.AddTransient<VendaPage>();
		builder.Services.AddTransient<VendaConcluidaPage>();
		builder.Services.AddTransient<SaldoPage>();
		builder.Services.AddTransient<PerfilPage>();
		builder.Services.AddTransient<TrocarSenhaPage>();
		builder.Services.AddTransient<DashboardPage>();
		builder.Services.AddTransient<SaldosPage>();
		builder.Services.AddTransient<AuditoriaPage>();
		builder.Services.AddTransient<GestaoPage>();
		builder.Services.AddTransient<UsuariosPage>();
		builder.Services.AddTransient<UsuarioFormPage>();
		builder.Services.AddTransient<ProdutosPage>();
		builder.Services.AddTransient<ProdutoFormPage>();
		builder.Services.AddTransient<EventosAdminPage>();
		builder.Services.AddTransient<EventoFormPage>();
		builder.Services.AddTransient<EventoDetalhePage>();
		builder.Services.AddTransient<EventoProdutoFormPage>();
		builder.Services.AddTransient<CartoesPage>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
