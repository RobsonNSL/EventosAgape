using AgapeApp.Aplicacao.Autenticacao.Dtos;
using MediatR;

namespace AgapeApp.Aplicacao.Autenticacao.Comandos.RenovarToken;

/// <summary>
/// Renova a sessão a partir de um token de renovação válido (com rotação do token).
/// </summary>
public record ComandoRenovarToken(
    string TokenRenovacao,
    string? Dispositivo = null) : IRequest<RespostaLogin>;
