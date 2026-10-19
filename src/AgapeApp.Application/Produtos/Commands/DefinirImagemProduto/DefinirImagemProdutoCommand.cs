using AgapeApp.Application.Produtos.DTOs;
using MediatR;

namespace AgapeApp.Application.Produtos.Commands.DefinirImagemProduto;

/// <summary>
/// Troca a imagem de um produto do catálogo. Somente administradores.
/// O conteúdo chega como stream (arquivo enviado pelo app) e é validado no handler.
/// </summary>
public record DefinirImagemProdutoCommand(
    Guid ProdutoId,
    string NomeArquivo,
    long Tamanho,
    Stream Conteudo) : IRequest<ProdutoDTO>;
