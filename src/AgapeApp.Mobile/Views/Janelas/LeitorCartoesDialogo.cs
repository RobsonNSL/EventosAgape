using AgapeApp.Mobile.Helpers;
using Microsoft.Maui.Controls.Shapes;
using ZXing.Net.Maui;
using ZXing.Net.Maui.Controls;

namespace AgapeApp.Mobile.Views.Janelas;

/// <summary>Resultado de um cartão lido no cadastro pela câmera.</summary>
public sealed record ResultadoLeitura(bool Sucesso, string Mensagem);

/// <summary>
/// Cadastro de cartões pela câmera, em sequência: cada QR lido é enviado na hora e a
/// janela continua aberta para o próximo, mostrando o resultado e a contagem.
/// Devolve quantos cartões foram cadastrados.
/// </summary>
public sealed class LeitorCartoesDialogo : DialogoBase<int>
{
    /// <summary>O mesmo QR parado na frente da câmera não é lido de novo antes disto.</summary>
    private static readonly TimeSpan RepeticaoIgnorada = TimeSpan.FromSeconds(4);

    private readonly Func<string, Task<ResultadoLeitura>> _aoLer;
    private readonly ContentView _areaCamera;
    private readonly Label _status;
    private readonly Label _contagem;
    private readonly Border _faixaResultado;
    private readonly Label _resultado;

    private CameraBarcodeReaderView? _leitor;
    private bool _processando;
    private string? _ultimoCodigo;
    private DateTime _ultimaLeitura = DateTime.MinValue;
    private int _cadastrados;

    public LeitorCartoesDialogo(Func<string, Task<ResultadoLeitura>> aoLer)
    {
        _aoLer = aoLer;

        var pilha = new VerticalStackLayout { Spacing = 12 };

        pilha.Add(UiDialogo.Titulo("Cadastrar pelo QR"));

        var instrucao = UiDialogo.Mensagem("Aponte para o QR de cada cartão. Cada leitura já cadastra o cartão; continue com os próximos e toque em Concluir no fim.");
        instrucao.FontSize = 14;
        pilha.Add(instrucao);

        _areaCamera = new ContentView();

        _status = new Label
        {
            Text = "Abrindo a câmera...",
            FontFamily = "InterMedium",
            FontSize = 13,
            TextColor = Cores.Superficie,
            HorizontalTextAlignment = TextAlignment.Center,
            VerticalOptions = LayoutOptions.Center,
            Margin = new Thickness(16)
        };

        var lanterna = new AgapeApp.Mobile.Controls.BotaoIcone
        {
            Glifo = "\uf0eb",
            TamanhoIcone = 17,
            CorIcone = Colors.White,
            BackgroundColor = Color.FromArgb("#CC0F172A"),
            StrokeThickness = 0,
            Margin = new Thickness(10),
            HorizontalOptions = LayoutOptions.End,
            VerticalOptions = LayoutOptions.Start
        };
        lanterna.Tocado += (_, _) =>
        {
            if (_leitor is null)
                return;

            _leitor.IsTorchOn = !_leitor.IsTorchOn;
            lanterna.CorIcone = _leitor.IsTorchOn ? Cores.Destaque : Colors.White;
        };

        pilha.Add(new Border
        {
            BackgroundColor = Cores.TextoPrimario,
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 16 },
            HeightRequest = Math.Min(300, AlturaTela * 0.38),
            Content = new Grid { Children = { _status, _areaCamera, lanterna } }
        });

        _resultado = new Label
        {
            FontFamily = "InterSemiBold",
            FontSize = 14,
            VerticalOptions = LayoutOptions.Center
        };

        _faixaResultado = new Border
        {
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 12 },
            Padding = new Thickness(12, 10),
            IsVisible = false,
            Content = _resultado
        };
        pilha.Add(_faixaResultado);

        _contagem = new Label
        {
            Text = "Nenhum cartão cadastrado ainda",
            FontFamily = "InterMedium",
            FontSize = 13,
            TextColor = Cores.TextoSecundario,
            HorizontalTextAlignment = TextAlignment.Center
        };
        pilha.Add(_contagem);

        var concluir = UiDialogo.Botao("Concluir", TomDialogo.Primario);
        concluir.Clicked += (_, _) => Seguro(() => FecharAsync(_cadastrados));
        pilha.Add(concluir);

        DefinirCorpo(pilha);
    }

    protected override int ResultadoCancelado => _cadastrados;

    protected override void AoApresentar() => Seguro(LigarCameraAsync);

    protected override void AoFechar() => DesligarCamera();

    private async Task LigarCameraAsync()
    {
        if (!BarcodeScanning.IsSupported)
        {
            _status.Text = "Este aparelho não tem câmera disponível. Use a digitação do código.";
            return;
        }

        var permissao = await Permissions.CheckStatusAsync<Permissions.Camera>();
        if (permissao != PermissionStatus.Granted)
            permissao = await Permissions.RequestAsync<Permissions.Camera>();

        if (permissao != PermissionStatus.Granted)
        {
            _status.Text = "Sem permissão para a câmera. Libere nos ajustes do aparelho.";
            return;
        }

        _leitor = new CameraBarcodeReaderView
        {
            Options = new BarcodeReaderOptions
            {
                Formats = BarcodeFormats.TwoDimensional | BarcodeFormats.OneDimensional,
                AutoRotate = true,
                Multiple = false,
                TryHarder = true
            }
        };

        _leitor.BarcodesDetected += AoDetectar;
        _areaCamera.Content = _leitor;
        _leitor.IsDetecting = true;
    }

    private void DesligarCamera()
    {
        if (_leitor is null)
            return;

        _leitor.IsDetecting = false;
        _leitor.IsTorchOn = false;
        _leitor.BarcodesDetected -= AoDetectar;

        // Tirar do layout desconecta o handler e encerra a câmera.
        _areaCamera.Content = null;
        _leitor = null;
    }

    private void AoDetectar(object? sender, BarcodeDetectionEventArgs e)
    {
        var lido = e.Results?.FirstOrDefault()?.Value;
        if (string.IsNullOrWhiteSpace(lido))
            return;

        MainThread.BeginInvokeOnMainThread(() => Seguro(() => ProcessarAsync(lido)));
    }

    private async Task ProcessarAsync(string lido)
    {
        var codigo = CodigoCartao.Normalizar(lido);

        if (_processando || string.IsNullOrWhiteSpace(codigo))
            return;

        if (codigo == _ultimoCodigo && DateTime.UtcNow - _ultimaLeitura < RepeticaoIgnorada)
            return;

        _processando = true;

        try
        {
            _ultimoCodigo = codigo;
            _ultimaLeitura = DateTime.UtcNow;

            MostrarResultado(new ResultadoLeitura(true, $"Cadastrando {codigo}..."), neutro: true);

            var resultado = await _aoLer(codigo);

            if (resultado.Sucesso)
            {
                _cadastrados++;
                _contagem.Text = _cadastrados == 1
                    ? "1 cartão cadastrado nesta leitura"
                    : $"{_cadastrados} cartões cadastrados nesta leitura";
            }

            MostrarResultado(resultado);
            Vibrar(resultado.Sucesso);
        }
        finally
        {
            _ultimaLeitura = DateTime.UtcNow;
            _processando = false;
        }
    }

    private void MostrarResultado(ResultadoLeitura resultado, bool neutro = false)
    {
        _faixaResultado.IsVisible = true;
        _resultado.Text = resultado.Mensagem;

        (_faixaResultado.BackgroundColor, _resultado.TextColor) = neutro
            ? (Cores.CinzaFundo, Cores.CinzaTexto)
            : resultado.Sucesso
                ? (Cores.VerdeFundo, Cores.VerdeTexto)
                : (Cores.PerigoFundo, Cores.PerigoTexto);
    }

    private static void Vibrar(bool sucesso)
    {
        try
        {
            HapticFeedback.Default.Perform(sucesso ? HapticFeedbackType.Click : HapticFeedbackType.LongPress);
        }
        catch (Exception)
        {
            // Aparelho sem vibração.
        }
    }
}
