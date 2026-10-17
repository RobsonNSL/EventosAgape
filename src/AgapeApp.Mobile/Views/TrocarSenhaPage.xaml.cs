using AgapeApp.Mobile.ViewModels;

namespace AgapeApp.Mobile.Views;

public partial class TrocarSenhaPage : ContentPage
{
    private readonly TrocarSenhaViewModel _vm;

    public TrocarSenhaPage(TrocarSenhaViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
        _vm.SenhaAlterada += AoSenhaAlterada;
    }

    private async void AoSenhaAlterada()
    {
        await DisplayAlertAsync(
            "Pronto!",
            "Senha alterada com sucesso. Use a nova senha no próximo login.",
            "OK");

        if (Shell.Current is not null)
            await Shell.Current.GoToAsync("..");
    }
}
