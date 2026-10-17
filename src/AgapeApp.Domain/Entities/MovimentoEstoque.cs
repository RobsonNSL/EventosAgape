using AgapeApp.Domain.Common;
using AgapeApp.Domain.Enums;
using AgapeApp.Domain.Exceptions;
using AgapeApp.Domain.Resources;

namespace AgapeApp.Domain.Entities;

/// <summary>
/// Movimentação de estoque de um produto do evento (estoque inicial, venda ou ajuste manual).
/// A quantidade é um delta: positiva em entradas e negativa em baixas por venda.
/// </summary>
public class MovimentoEstoque : BaseEntity
{
    // Construtor privado exigido pelo Entity Framework.
    private MovimentoEstoque()
    {
    }

    public Guid EventoProdutoId { get; private set; }

    public int Quantidade { get; private set; }

    public MotivoMovimentoEstoque Motivo { get; private set; }

    /// <summary>Preenchido quando o movimento é uma baixa por venda.</summary>
    public Guid? VendaId { get; private set; }

    public Guid UsuarioId { get; private set; }

    public string? Observacao { get; private set; }

    public static MovimentoEstoque Criar(
        Guid eventoProdutoId,
        int quantidade,
        MotivoMovimentoEstoque motivo,
        Guid usuarioId,
        Guid? vendaId = null,
        string? observacao = null)
    {
        if (quantidade == 0)
            throw new RegraNegocioException(MensagensErro.AjusteEstoqueInvalido);

        return new MovimentoEstoque
        {
            EventoProdutoId = eventoProdutoId,
            Quantidade = quantidade,
            Motivo = motivo,
            VendaId = vendaId,
            UsuarioId = usuarioId,
            Observacao = string.IsNullOrWhiteSpace(observacao) ? null : observacao.Trim()
        };
    }
}
