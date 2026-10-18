using AgapeApp.Mobile.Helpers;
using AgapeApp.Mobile.ViewModels;
using ZXing.Net.Maui;
using ZXing.Net.Maui.Controls;

namespace AgapeApp.Mobile.Views;

public partial class LeituraQrPage : ContentPage, IQueryAttributable
{
    private static readonly TimeSpan IntervaloEntreLeituras = TimeSpan.FromSeconds(2);

    private readonly LeituraQrViewModel _vm;

    private CameraBarcodeReaderView? _leitor;
    private DateTime _ultimaLeitura = DateTime.MinValue;
    private bool _processando;

    public LeituraQrPage(LeituraQrViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
        => _vm.AplicarParametros(query);

    /// <summary>Chamado ao voltar de uma operação concluída ("próximo cliente").</summary>
    public void PrepararNovaLeitura(string origem) => _vm.PrepararNovaLeitura(origem);

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Cada volta para esta tela é um cartão novo.
        _vm.Codigo = string.Empty;
        _vm.MensagemErro = null;

        await IniciarCameraAsync();
    }

    protected override void OnDisappearing()
    {
        PararCamera();
        _ultimaLeitura = DateTime.MinValue;
        base.OnDisappearing();
    }

    private async Task IniciarCameraAsync()
    {
        try
        {
            if (!BarcodeScanning.IsSupported)
            {
                StatusCamera.Text = "Este aparelho não tem câmera disponível. Digite o código abaixo.";
                return;
            }

            var permissao = await Permissions.CheckStatusAsync<Permissions.Camera>();
            if (permissao != PermissionStatus.Granted)
                permissao = await Permissions.RequestAsync<Permissions.Camera>();

            if (permissao != PermissionStatus.Granted)
            {
                StatusCamera.Text = "Sem permissão para a câmera. Libere nos ajustes do aparelho ou digite o código abaixo.";
                return;
            }

            LigarCamera();
        }
        catch
        {
            StatusCamera.Text = "Não foi possível abrir a câmera. Digite o código abaixo.";
        }
    }

    private void LigarCamera()
    {
        _leitor = new CameraBarcodeReaderView
        {
            // QR do cartão (e código de barras, caso o cartão tenha os dois).
            Options = new BarcodeReaderOptions
            {
                Formats = BarcodeFormats.TwoDimensional | BarcodeFormats.OneDimensional,
                AutoRotate = true,
                Multiple = false,
                TryHarder = true,
                DelayBetweenContinuousScans = 1500
            }
        };

        _leitor.BarcodesDetected += AoDetectarCodigo;
        CameraHost.Content = _leitor;
        _leitor.IsDetecting = true;

        StatusCamera.Text = "Câmera ativa — a leitura confirma o cartão automaticamente.";
    }

    private void PararCamera()
    {
        if (_leitor is null)
            return;

        _leitor.IsDetecting = false;
        _leitor.IsTorchOn = false;
        _leitor.BarcodesDetected -= AoDetectarCodigo;

        // Tirar do layout desconecta o handler e encerra a câmera.
        CameraHost.Content = null;
        _leitor = null;
    }

    private void AoDetectarCodigo(object? sender, BarcodeDetectionEventArgs e)
    {
        var lido = e.Results?.FirstOrDefault()?.Value;
        if (string.IsNullOrWhiteSpace(lido))
            return;

        MainThread.BeginInvokeOnMainThread(() => ProcessarAsync(lido));
    }

    private async void ProcessarAsync(string lido)
    {
        if (_processando || DateTime.UtcNow - _ultimaLeitura < IntervaloEntreLeituras)
            return;

        _processando = true;
        try
        {
            if (_leitor is not null)
                _leitor.IsDetecting = false;

            _ultimaLeitura = DateTime.UtcNow;

            var codigo = CodigoCartao.Normalizar(lido);
            if (string.IsNullOrWhiteSpace(codigo))
                return;

            _vm.Codigo = codigo;

            try
            {
                HapticFeedback.Default.Perform(HapticFeedbackType.Click);
            }
            catch
            {
                // Aparelho sem motor de vibração: segue sem o toque.
            }

            await _vm.ConfirmarCommand.ExecuteAsync(null);
        }
        finally
        {
            _processando = false;
        }
    }

    private void AoAlternarLanterna(object? sender, EventArgs e)
    {
        if (_leitor is null)
            return;

        _leitor.IsTorchOn = !_leitor.IsTorchOn;
        BotaoLanterna.CorIcone = _leitor.IsTorchOn ? Color.FromArgb("#F59E0B") : Colors.White;
    }

}
