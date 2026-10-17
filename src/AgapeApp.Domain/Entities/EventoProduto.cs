using AgapeApp.Domain.Common;
using AgapeApp.Domain.Exceptions;
using AgapeApp.Domain.Resources;

namespace AgapeApp.Domain.Entities;

/// <summary>
/// Vínculo produto × evento: define o preço praticado no evento (igual em todos os setores)
/// e, opcionalmente, o controle de estoque (compartilhado entre os setores que vendem o produto).
/// </summary>
public class EventoProduto : BaseEntity
{
    // Construtor privado exigido pelo Entity Framework.
    private EventoProduto()
    {
    }

    public Guid EventoId { get; private set; }

    public Guid ProdutoId { get; private set; }

    public decimal Preco { get; private set; }

    public bool ControlaEstoque { get; private set; }

    public int QuantidadeEstoque { get; private set; }

    /// <summary>
    /// Vincula um produto ao evento com o preço praticado e, se controlar estoque, a quantidade inicial.
    /// </summary>
    public static EventoProduto Criar(
        Guid eventoId,
        Guid produtoId,
        decimal preco,
        bool controlaEstoque,
        int quantidadeEstoqueInicial)
    {
        ValidarPreco(preco);
        ValidarQuantidade(quantidadeEstoqueInicial);

        return new EventoProduto
        {
            EventoId = eventoId,
            ProdutoId = produtoId,
            Preco = preco,
            ControlaEstoque = controlaEstoque,
            QuantidadeEstoque = controlaEstoque ? quantidadeEstoqueInicial : 0
        };
    }

    /// <summary>
    /// Atualiza o preço praticado e o controle de estoque do produto no evento.
    /// O estoque já registrado é mantido (apenas ignorado quando o controle está desligado).
    /// </summary>
    public void Atualizar(decimal preco, bool controlaEstoque)
    {
        ValidarPreco(preco);

        Preco = preco;
        ControlaEstoque = controlaEstoque;
        MarcarComoAtualizada();
    }

    /// <summary>
    /// Ajuste manual do estoque (ex.: comprar mais refrigerante no meio do evento).
    /// </summary>
    public void AjustarEstoque(int quantidade)
    {
        if (!ControlaEstoque)
            throw new RegraNegocioException(MensagensErro.EventoProdutoSemControleEstoque);

        if (quantidade == 0)
            throw new RegraNegocioException(MensagensErro.AjusteEstoqueInvalido);

        if (QuantidadeEstoque + quantidade < 0)
            throw new RegraNegocioException(MensagensErro.AjusteEstoqueNegativo);

        QuantidadeEstoque += quantidade;
        MarcarComoAtualizada();
    }

    /// <summary>
    /// Baixa de estoque por venda.
    /// </summary>
    public void RetirarEstoque(int quantidade)
    {
        if (quantidade <= 0)
            throw new RegraNegocioException(MensagensErro.QuantidadeInvalida);

        if (QuantidadeEstoque - quantidade < 0)
            throw new RegraNegocioException(MensagensErro.EstoqueNegativo);

        QuantidadeEstoque -= quantidade;
        MarcarComoAtualizada();
    }

    private static void ValidarPreco(decimal preco)
    {
        if (preco <= 0)
            throw new RegraNegocioException(MensagensErro.PrecoInvalido);
    }

    private static void ValidarQuantidade(int quantidade)
    {
        if (quantidade < 0)
            throw new RegraNegocioException(MensagensErro.QuantidadeEstoqueNegativa);
    }
}
