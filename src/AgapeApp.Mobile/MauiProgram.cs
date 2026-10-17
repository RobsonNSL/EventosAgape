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
		builder.Services.AddSingleton(new HttpClient { Timeout = TimeSpan.FromSeconds(30) });
		builder.Services.AddSingleton<SessaoService>();
		builder.Services.AddSingleton<ApiClient>();
		builder.Services.AddSingleton<CacheService>();
		builder.Services.AddSingleton<DashboardService>();
		builder.Services.AddSingleton<IBiometric>(_ => BiometricAuthenticationService.Default);
		builder.Services.AddSingleton<BiometriaService>();
		builder.Services.AddSingleton<AppShell>();

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

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
