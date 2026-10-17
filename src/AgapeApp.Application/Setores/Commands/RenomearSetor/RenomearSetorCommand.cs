using AgapeApp.Application.Setores.DTOs;
using MediatR;

namespace AgapeApp.Application.Setores.Commands.RenomearSetor;

/// <summary>
/// Renomeia um setor (não permitido em evento encerrado). Somente administradores.
/// </summary>
public record RenomearSetorCommand(
    Guid Id,
    string Nome) : IRequest<SetorDTO>;
