using AgapeApp.Mobile.ViewModels;

namespace AgapeApp.Mobile.Views;

public partial class GestaoPage : ContentPage
{
    public GestaoPage(GestaoViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
