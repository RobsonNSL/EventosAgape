using System.Text;

namespace AgapeApp.Mobile.Helpers;

/// <summary>
/// Guarda o último erro não tratado num arquivo do aparelho. Em vez de o app só "fechar
/// sozinho", o erro fica registrado e é mostrado na próxima abertura — dá para mandar a
/// mensagem para quem mantém o app. Erros vindos de eventos da tela (toques, janelas)
/// são absorvidos no Android para o app não cair no meio de uma operação.
/// </summary>
public static class RegistroErros
{
    private static readonly object Trava = new();

    private static string Caminho => Path.Combine(FileSystem.AppDataDirectory, "ultimo-erro.txt");

    public static void Iniciar()
    {
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
            Registrar(e.ExceptionObject as Exception, "AppDomain");

        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            Registrar(e.Exception, "Task");
            e.SetObserved();
        };

#if ANDROID
        Android.Runtime.AndroidEnvironment.UnhandledExceptionRaiser += (_, e) =>
        {
            Registrar(e.Exception, "Android");

            // Exceção de um toque/janela (async void): avisa e mantém o app aberto.
            e.Handled = true;
            Notificacao.Mostrar("Algo deu errado nesta tela. O erro foi registrado; tente de novo.");
        };
#endif
    }

    public static void Registrar(Exception? erro, string origem)
    {
        if (erro is null)
            return;

        try
        {
            var texto = new StringBuilder()
                .AppendLine($"{DateTime.Now:dd/MM/yyyy HH:mm:ss} · {origem} · v{AppInfo.Current.VersionString}")
                .AppendLine(erro.ToString())
                .ToString();

            lock (Trava)
                File.WriteAllText(Caminho, texto);
        }
        catch (Exception)
        {
            // Sem como gravar: nada a fazer.
        }
    }

    /// <summary>Devolve (e apaga) o erro registrado na sessão anterior.</summary>
    public static string? LerEApagar()
    {
        try
        {
            lock (Trava)
            {
                if (!File.Exists(Caminho))
                    return null;

                var texto = File.ReadAllText(Caminho);
                File.Delete(Caminho);
                return string.IsNullOrWhiteSpace(texto) ? null : texto;
            }
        }
        catch (Exception)
        {
            return null;
        }
    }
}
