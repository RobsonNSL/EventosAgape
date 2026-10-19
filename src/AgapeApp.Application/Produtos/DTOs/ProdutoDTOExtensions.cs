using AgapeApp.Domain.Entities;

namespace AgapeApp.Application.Produtos.DTOs;

/// <summary>
/// Conversão de <see cref="Produto"/> para <see cref="ProdutoDTO"/>.
/// </summary>
public static class ProdutoDTOExtensions
{
    public static ProdutoDTO ParaDTO(this Produto produto) => new(
        produto.Id,
        produto.Nome,
        produto.PrecoPadrao,
        produto.Ativo,
        produto.CriadoEm,
        produto.CaminhoImagem);
}
