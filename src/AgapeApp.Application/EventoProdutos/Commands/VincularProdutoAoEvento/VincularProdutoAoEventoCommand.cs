using AgapeApp.Application.EventoProdutos.DTOs;
using MediatR;

namespace AgapeApp.Application.EventoProdutos.Commands.VincularProdutoAoEvento;

/// <summary>
/// Vincula um produto ao evento com o preço praticado e, se controlar estoque, a quantidade inicial.
/// O campo UsuarioId é preenchido pela API com o usuário autenticado. Somente administradores.
/// </summary>
public record VincularProdutoAoEventoCommand(
    Guid EventoId,
    Guid ProdutoId,
    decimal Preco,
    bool ControlaEstoque,
    int QuantidadeEstoqueInicial,
    Guid UsuarioId) : IRequest<EventoProdutoDTO>;
