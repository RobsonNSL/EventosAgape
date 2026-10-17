using AgapeApp.Application.Usuarios.DTOs;
using MediatR;

namespace AgapeApp.Application.Authentication.Commands.DefinirMinhaFoto;

/// <summary>
/// Troca a foto de perfil do próprio usuário autenticado.
/// O conteúdo chega como stream (arquivo enviado pelo app) e é validado no handler.
/// </summary>
public record DefinirMinhaFotoCommand(
    Guid UsuarioId,
    string NomeArquivo,
    long Tamanho,
    Stream Conteudo) : IRequest<UsuarioDTO>;
