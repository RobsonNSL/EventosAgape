using AgapeApp.Mobile.Services;
using CommunityToolkit.Mvvm.Input;

namespace AgapeApp.Mobile.ViewModels;

/// <summary>
/// Área de gestão (administrador): atalhos para usuários, produtos, eventos e cartões.
/// Um operador não chega aqui: a aba só aparece para administradores.
/// </summary>
public partial class GestaoViewModel : BaseViewModel
{
    public GestaoViewModel(ApiClient api, SessaoService sessao) : base(api, sessao)
    {
    }

    [RelayCommand]
    private Task AbrirUsuariosAsync() => NavegarAsync("usuarios");

    [RelayCommand]
    private Task AbrirProdutosAsync() => NavegarAsync("produtos");

    [RelayCommand]
    private Task AbrirEventosAsync() => NavegarAsync("eventos-admin");

    [RelayCommand]
    private Task AbrirCartoesAsync() => NavegarAsync("cartoes");
}
