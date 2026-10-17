using AgapeApp.Mobile.Helpers;
using AgapeApp.Mobile.Models;
using AgapeApp.Mobile.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AgapeApp.Mobile.ViewModels;

/// <summary>Cadastro e edição de evento (administrador).</summary>
public partial class EventoFormViewModel : BaseViewModel
{
    private Guid _id;

    public EventoFormViewModel(ApiClient api, SessaoService sessao) : base(api, sessao)
    {
    }

    [ObservableProperty]
    private string nome = string.Empty;

    [ObservableProperty]
    private DateTime dataInicio = DateTime.Today;

    [ObservableProperty]
    private DateTime dataFim = DateTime.Today;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(MostrarDataFim))]
    private bool terminaOutroDia;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Titulo), nameof(Subtitulo), nameof(TextoBotao))]
    private bool ehNovo = true;

    public bool MostrarDataFim => TerminaOutroDia;

    public string Titulo => EhNovo ? "Novo evento" : "Editar evento";

    public string Subtitulo => EhNovo
        ? "O evento é criado em preparação; depois você abre para vendas"
        : "Atualize o nome e as datas do evento";

    public string TextoBotao => EhNovo ? "Criar evento" : "Salvar alterações";

    public void AplicarParametros(IDictionary<string, object> parametros)
    {
        var evento = Parametros.Objeto<EventoAdminItem>(parametros, "evento");

        if (evento is null)
        {
            EhNovo = true;
            DataInicio = DateTime.Today;
            DataFim = DateTime.Today;
            return;
        }

        _id = evento.Id;
        Nome = evento.Nome;
        DataInicio = evento.DataInicio.ToDateTime(TimeOnly.MinValue);
        TerminaOutroDia = evento.DataFim is not null && evento.DataFim != evento.DataInicio;
        DataFim = (evento.DataFim ?? evento.DataInicio).ToDateTime(TimeOnly.MinValue);
        EhNovo = false;
    }

    [RelayCommand]
    private async Task SalvarAsync()
    {
        if (string.IsNullOrWhiteSpace(Nome))
        {
            MensagemErro = "Informe o nome do evento.";
            return;
        }

        var inicio = DateOnly.FromDateTime(DataInicio);
        var fim = TerminaOutroDia ? DateOnly.FromDateTime(DataFim) : (DateOnly?)null;

        if (fim is not null && fim < inicio)
        {
            MensagemErro = "A data final não pode ser antes da data inicial.";
            return;
        }

        await ExecutarAsync(async () =>
        {
            if (EhNovo)
                await Api.PostAsync<EventoDTO>(
                    "api/eventos",
                    new { nome = Nome, dataInicio = inicio, dataFim = fim });
            else
                await Api.PutAsync<EventoDTO>(
                    $"api/eventos/{_id}",
                    new { nome = Nome, dataInicio = inicio, dataFim = fim });

            await NavegarAsync("..");
        });
    }
}
