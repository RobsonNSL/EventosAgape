using System.ComponentModel;
using AgapeApp.Mobile.Helpers;
using AgapeApp.Mobile.ViewModels;
using ZXing.Net.Maui;
using ZXing.Net.Maui.Controls;

namespace AgapeApp.Mobile.Views;

/// <summary>
/// Leitura do cartão. A câmera só liga quando o operador escolhe "Ler o QR code" e lê
/// apenas QR: com códigos de barras (1D) ligados e TryHarder, a câmera "achava" números
/// em qualquer padrão da cena (ex.: "21") e abria um cartão que nem existe.
/// Uma leitura só vale quando o mesmo QR aparece duas vezes seguidas, e o cartão é
/// conferido na API antes de abrir a operação.
/// </summary>
public partial class LeituraQrPage : ContentPage, IQueryAttributable
{
    /// <summary>A segunda leitura igual precisa vir dentro deste intervalo.</summary>
    private static readonly TimeSpan JanelaConfirmacao = TimeSpan.FromSeconds(1.5);

    /// <summary>Cartão recusado (não existe, bloqueado) não é tentado de novo antes disto.</summary>
    private static readonly TimeSpan PausaAposRecusa = TimeSpan.FromSeconds(3);

    private readonly LeituraQrViewModel _vm;

    private CameraBarcodeReaderView? _leitor;
    private bool _processando;
    private string? _candidato;
    private DateTime _candidatoEm = DateTime.MinValue;
    private string? _recusado;
    private DateTime _recusadoEm = DateTime.MinValue;

    public LeituraQrPage(LeituraQrViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
        _vm.PropertyChanged += AoMudarViewModel;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
        => _vm.AplicarParametros(query);

    /// <summary>Chamado ao voltar de uma operação concluída ("próximo cliente").</summary>
    public void PrepararNovaLeitura(string origem) => _vm.PrepararNovaLeitura(origem);

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Cada volta para esta tela é um cartão novo; o modo escolhido (câmera ou
        // digitação) continua o mesmo para o próximo cliente.
        _vm.Codigo = string.Empty;
        _vm.MensagemErro = null;
        LimparLeitura();

        if (_vm.MostrarCamera)
            await IniciarCameraAsync();
    }

    protected override void OnDisappearing()
    {
        PararCamera();
        base.OnDisappearing();
    }

    private async void AoMudarViewModel(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(LeituraQrViewModel.Modo))
            return;

        try
        {
            if (_vm.MostrarCamera)
            {
                await IniciarCameraAsync();
                return;
            }

            PararCamera();

            if (_vm.MostrarManual)
                Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(150), () => CampoCodigo.Focus());
        }
        catch (Exception erro)
        {
            RegistroErros.Registrar(erro, "LeituraModo");
        }
    }

    private async Task IniciarCameraAsync()
    {
        if (_leitor is not null)
            return;

        try
        {
            if (!BarcodeScanning.IsSupported)
            {
                StatusCamera.Text = "Este aparelho não tem câmera disponível. Use \"Digitar o código\".";
                return;
            }

            var permissao = await Permissions.CheckStatusAsync<Permissions.Camera>();
            if (permissao != PermissionStatus.Granted)
                permissao = await Permissions.RequestAsync<Permissions.Camera>();

            if (permissao != PermissionStatus.Granted)
            {
                StatusCamera.Text = "Sem permissão para a câmera. Libere nos ajustes do aparelho ou digite o código.";
                return;
            }

            // A pessoa pode ter trocado para digitação enquanto a permissão era pedida.
            if (!_vm.MostrarCamera || _leitor is not null)
                return;

            LigarCamera();
        }
        catch (Exception erro)
        {
            RegistroErros.Registrar(erro, "LeituraCamera");
            StatusCamera.Text = "Não foi possível abrir a câmera. Digite o código.";
        }
    }

    private void LigarCamera()
    {
        _leitor = new CameraBarcodeReaderView
        {
            Options = new BarcodeReaderOptions
            {
                Formats = BarcodeFormat.QrCode,
                AutoRotate = true,
                Multiple = false,
                TryHarder = false
            }
        };

        _leitor.BarcodesDetected += AoDetectarCodigo;
        CameraHost.Content = _leitor;
        _leitor.IsDetecting = true;

        StatusCamera.Text = "Câmera ativa — segure o QR parado por um instante.";
    }

    private void PararCamera()
    {
        if (_leitor is null)
            return;

        _leitor.IsDetecting = false;
        _leitor.IsTorchOn = false;
        _leitor.BarcodesDetected -= AoDetectarCodigo;
        BotaoLanterna.CorIcone = Colors.White;

        // Tirar do layout desconecta o handler e encerra a câmera.
        CameraHost.Content = null;
        _leitor = null;
    }

    private void AoDetectarCodigo(object? sender, BarcodeDetectionEventArgs e)
    {
        var resultado = e.Results?.FirstOrDefault();
        if (resultado is null || resultado.Format != BarcodeFormat.QrCode || string.IsNullOrWhiteSpace(resultado.Value))
            return;

        var lido = resultado.Value;
        MainThread.BeginInvokeOnMainThread(() => ProcessarAsync(lido));
    }

    private async void ProcessarAsync(string lido)
    {
        try
        {
            var codigo = CodigoCartao.Normalizar(lido);

            if (_processando || codigo.Length < 3)
                return;

            if (codigo == _recusado && DateTime.UtcNow - _recusadoEm < PausaAposRecusa)
                return;

            // Primeira leitura só marca o candidato; a segunda igual confirma.
            if (codigo != _candidato || DateTime.UtcNow - _candidatoEm > JanelaConfirmacao)
            {
                _candidato = codigo;
                _candidatoEm = DateTime.UtcNow;
                return;
            }

            _processando = true;

            if (_leitor is not null)
                _leitor.IsDetecting = false;

            StatusCamera.Text = $"Conferindo o cartão {codigo}...";
            Vibrar();

            var abriu = await _vm.ConfirmarCodigoAsync(codigo);

            if (!abriu)
            {
                _recusado = codigo;
                _recusadoEm = DateTime.UtcNow;
                StatusCamera.Text = "Câmera ativa — aponte para outro cartão ou digite o código.";

                if (_leitor is not null)
                    _leitor.IsDetecting = true;
            }

            LimparCandidato();
        }
        catch (Exception erro)
        {
            RegistroErros.Registrar(erro, "LeituraQr");
        }
        finally
        {
            _processando = false;
        }
    }

    private void LimparLeitura()
    {
        LimparCandidato();
        _recusado = null;
        _recusadoEm = DateTime.MinValue;
    }

    private void LimparCandidato()
    {
        _candidato = null;
        _candidatoEm = DateTime.MinValue;
    }

    private static void Vibrar()
    {
        try
        {
            HapticFeedback.Default.Perform(HapticFeedbackType.Click);
        }
        catch (Exception)
        {
            // Aparelho sem motor de vibração: segue sem o toque.
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
