using Microsoft.Extensions.DependencyInjection;

namespace AgapeApp.Mobile;

public partial class App : Application
{
	private readonly IServiceProvider _servicos;

	public App(IServiceProvider servicos)
	{
		InitializeComponent();
		_servicos = servicos;
	}

	protected override Window CreateWindow(IActivationState? activationState)
		=> new(_servicos.GetRequiredService<AppShell>());
}
