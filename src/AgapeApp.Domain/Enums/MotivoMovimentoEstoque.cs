namespace AgapeApp.Domain.Enums;

/// <summary>
/// Motivo de uma movimentação de estoque.
/// </summary>
public enum MotivoMovimentoEstoque
{
    /// <summary>Estoque inicial definido ao vincular o produto ao evento.</summary>
    EstoqueInicial = 1,

    /// <summary>Baixa automática por venda (quantidade negativa).</summary>
    Venda = 2,

    /// <summary>Ajuste manual feito pelo administrador no meio do evento.</summary>
    AjusteManual = 3
}
