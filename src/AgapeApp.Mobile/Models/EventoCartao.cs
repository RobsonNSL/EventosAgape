using AgapeApp.Mobile.Helpers;

namespace AgapeApp.Mobile.Models;

/// <summary>Evento pronto para exibição na lista de eventos (cores e textos calculados).</summary>
public class EventoCartao
{
    public required Guid EventoId { get; init; }
    public required Guid SetorId { get; init; }
    public required string Nome { get; init; }
    public required string SetorNome { get; init; }
    public required string Situacao { get; init; }
    public required bool Habilitado { get; init; }
    public required string DataTexto { get; init; }
    public required string SituacaoTexto { get; init; }
    public required Color SituacaoFundo { get; init; }
    public required Color SituacaoCor { get; init; }

    /// <summary>Administrador pode abrir o dashboard do evento.</summary>
    public bool MostrarDashboard { get; init; }

    /// <summary>Há setor definido (visão "meus eventos"); sem setor, não dá para operar.</summary>
    public bool MostrarOperar => SetorId != Guid.Empty;

    public bool MostrarSetor => !string.IsNullOrWhiteSpace(SetorNome);

    public bool PodeOperar => MostrarOperar && Habilitado && Situacao == "Aberto";

    public bool MostrarAvisoLiberacao => MostrarOperar && !Habilitado;

    public string TextoBotao => !Habilitado
        ? "Aguardando liberação"
        : Situacao switch
        {
            "Aberto" => "Operar terminal",
            "Encerrado" => "Evento encerrado",
            _ => "Ainda não aberto para vendas"
        };

    public static EventoCartao De(MeuEventoDTO dto, bool mostrarDashboard = false) => new()
    {
        EventoId = dto.EventoId,
        SetorId = dto.SetorId,
        Nome = dto.Nome,
        SetorNome = dto.SetorNome,
        Situacao = dto.Situacao,
        Habilitado = dto.Habilitado,
        MostrarDashboard = mostrarDashboard,
        DataTexto = Formatos.DataEvento(dto.DataInicio, dto.DataFim),
        SituacaoTexto = TextoDaSituacao(dto.Situacao),
        SituacaoFundo = FundoDaSituacao(dto.Situacao),
        SituacaoCor = CorDaSituacao(dto.Situacao)
    };

    /// <summary>Evento na visão do administrador (todos os eventos, sem setor do operador).</summary>
    public static EventoCartao DeAdmin(EventoDTO dto) => new()
    {
        EventoId = dto.Id,
        SetorId = Guid.Empty,
        Nome = dto.Nome,
        SetorNome = string.Empty,
        Situacao = dto.Situacao,
        Habilitado = false,
        MostrarDashboard = true,
        DataTexto = Formatos.DataEvento(dto.DataInicio, dto.DataFim),
        SituacaoTexto = TextoDaSituacao(dto.Situacao),
        SituacaoFundo = FundoDaSituacao(dto.Situacao),
        SituacaoCor = CorDaSituacao(dto.Situacao)
    };

    private static string TextoDaSituacao(string situacao) => situacao switch
    {
        "Aberto" => "Aberto",
        "Encerrado" => "Encerrado",
        _ => "Em preparação"
    };

    private static Color FundoDaSituacao(string situacao) => situacao switch
    {
        "Aberto" => Cores.VerdeFundo,
        "Encerrado" => Cores.CinzaFundo,
        _ => Cores.AmbarFundo
    };

    private static Color CorDaSituacao(string situacao) => situacao switch
    {
        "Aberto" => Cores.VerdeTexto,
        "Encerrado" => Cores.CinzaTexto,
        _ => Cores.AmbarTexto
    };
}
