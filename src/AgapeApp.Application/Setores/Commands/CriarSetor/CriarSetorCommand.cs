using AgapeApp.Application.Setores.DTOs;
using MediatR;

namespace AgapeApp.Application.Setores.Commands.CriarSetor;

/// <summary>
/// Cria um setor em um evento (não permitido em evento encerrado). Somente administradores.
/// </summary>
public record CriarSetorCommand(
    Guid EventoId,
    string Nome) : IRequest<SetorDTO>;
