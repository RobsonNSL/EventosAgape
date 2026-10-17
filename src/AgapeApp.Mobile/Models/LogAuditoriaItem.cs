using AgapeApp.Mobile.Helpers;

namespace AgapeApp.Mobile.Models;

/// <summary>Registro de auditoria pronto para exibição (ícone, cor e data local).</summary>
public class LogAuditoriaItem
{
    public required string Acao { get; init; }
    public required string AcaoTexto { get; init; }
    public required string UsuarioNome { get; init; }
    public required string DataTexto { get; init; }
    public string? Dispositivo { get; init; }

    public bool TemDispositivo => !string.IsNullOrWhiteSpace(Dispositivo);

    public required string Icone { get; init; }
    public required Color Cor { get; init; }
    public required Color Fundo { get; init; }

    public static LogAuditoriaItem De(LogAuditoriaDTO dto)
    {
        var (texto, icone, cor, fundo) = Mapear(dto.Acao);

        return new LogAuditoriaItem
        {
            Acao = dto.Acao,
            AcaoTexto = texto,
            UsuarioNome = dto.UsuarioNome,
            DataTexto = dto.CriadoEm.ToLocalTime().ToString("dd/MM 'às' HH:mm", Formatos.Cultura),
            Dispositivo = dto.Dispositivo,
            Icone = icone,
            Cor = cor,
            Fundo = fundo
        };
    }

    private static (string Texto, string Icone, Color Cor, Color Fundo) Mapear(string acao) => acao switch
    {
        "Recarga" => ("Recarga", "\uf0d6", Cores.AmbarTexto, Cores.AmbarFundo),
        "Venda" => ("Venda", "\uf07a", Cores.VerdeTexto, Cores.VerdeFundo),
        "CriarEvento" => ("Evento criado", "\uf067", Cores.AzulTexto, Cores.AzulFundo),
        "AbrirEvento" => ("Evento aberto", "\uf3c1", Cores.AzulTexto, Cores.AzulFundo),
        "EncerrarEvento" => ("Evento encerrado", "\uf023", Cores.CinzaTexto, Cores.CinzaFundo),
        "DesignarOperador" => ("Operador designado", "\uf234", Cores.AzulTexto, Cores.AzulFundo),
        "AtualizarDesignacaoOperador" => ("Designação atualizada", "\uf044", Cores.CinzaTexto, Cores.CinzaFundo),
        "AjusteEstoque" => ("Ajuste de estoque", "\uf1b2", Cores.AmbarTexto, Cores.AmbarFundo),
        "AtualizarEventoProduto" => ("Produto atualizado", "\uf044", Cores.CinzaTexto, Cores.CinzaFundo),
        _ => (acao, "\uf05a", Cores.CinzaTexto, Cores.CinzaFundo)
    };
}
