using AgapeApp.Domain.Common;

namespace AgapeApp.Domain.Entities;

/// <summary>
/// Registro de auditoria: quem fez, o quê, quando e em qual dispositivo.
/// Alimenta o relatório de auditoria do dashboard do admin.
/// </summary>
public class LogAuditoria : BaseEntity
{
    // Construtor privado exigido pelo Entity Framework.
    private LogAuditoria()
    {
    }

    public Guid UsuarioId { get; private set; }

    /// <summary>Ação executada (ex.: Recarga, Venda, AjusteEstoque, AbrirEvento).</summary>
    public string Acao { get; private set; } = null!;

    /// <summary>Entidade afetada (ex.: Recarga, Venda, EventoProduto, Evento).</summary>
    public string Entidade { get; private set; } = null!;

    public Guid EntidadeId { get; private set; }

    /// <summary>Evento relacionado (quando a ação é vinculada a um evento), facilita os relatórios.</summary>
    public Guid? EventoId { get; private set; }

    /// <summary>Detalhes livres em JSON (valores, quantidades, etc.).</summary>
    public string? Dados { get; private set; }

    public string? Dispositivo { get; private set; }

    public static LogAuditoria Criar(
        Guid usuarioId,
        string acao,
        string entidade,
        Guid entidadeId,
        Guid? eventoId = null,
        string? dados = null,
        string? dispositivo = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(acao);
        ArgumentException.ThrowIfNullOrWhiteSpace(entidade);

        return new LogAuditoria
        {
            UsuarioId = usuarioId,
            Acao = acao.Trim(),
            Entidade = entidade.Trim(),
            EntidadeId = entidadeId,
            EventoId = eventoId,
            Dados = string.IsNullOrWhiteSpace(dados) ? null : dados.Trim(),
            Dispositivo = string.IsNullOrWhiteSpace(dispositivo) ? null : dispositivo.Trim()
        };
    }
}
