namespace AgapeApp.Mobile.Helpers;

/// <summary>
/// Aviso rápido que some sozinho ("Operador designado", "Produto salvo"...), para
/// confirmar uma ação sem obrigar o usuário a tocar em "OK".
/// </summary>
public static class Notificacao
{
    public static void Mostrar(string mensagem)
    {
        if (string.IsNullOrWhiteSpace(mensagem))
            return;

        MainThread.BeginInvokeOnMainThread(() =>
        {
            try
            {
#if ANDROID
                Android.Widget.Toast.MakeText(Platform.AppContext, mensagem, Android.Widget.ToastLength.Short)?.Show();
#endif
            }
            catch (Exception)
            {
                // Aviso é cortesia: nunca derruba a operação.
            }
        });
    }
}
