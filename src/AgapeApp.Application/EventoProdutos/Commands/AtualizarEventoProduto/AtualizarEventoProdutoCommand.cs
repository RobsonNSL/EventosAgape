using AgapeApp.Application.EventoProdutos.DTOs;
using MediatR;

namespace AgapeApp.Application.EventoProdutos.Commands.AtualizarEventoProduto;

/// <summary>
/// Atualiza o preço praticado e o controle de estoque de um produto no evento.
/// O campo UsuarioId é preenchido pela API com o usuário autenticado. Somente administradores.
/// </summary>
public record AtualizarEventoProdutoCommand(
    Guid Id,
    decimal Preco,
    bool ControlaEstoque,
    Guid UsuarioId) : IRequest<EventoProdutoDTO>;
