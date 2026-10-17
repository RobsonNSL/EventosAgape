using AgapeApp.Application.SetorProdutos.DTOs;
using MediatR;

namespace AgapeApp.Application.SetorProdutos.Commands.VincularProdutoAoSetor;

/// <summary>
/// Vincula um produto do evento a um setor (define o que o setor vende).
/// Não permitido em evento encerrado. Somente administradores.
/// </summary>
public record VincularProdutoAoSetorCommand(
    Guid SetorId,
    Guid EventoProdutoId) : IRequest<SetorProdutoDTO>;
