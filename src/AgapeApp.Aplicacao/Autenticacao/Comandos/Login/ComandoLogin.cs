using AgapeApp.Aplicacao.Autenticacao.Dtos;
using MediatR;

namespace AgapeApp.Aplicacao.Autenticacao.Comandos.Login;

/// <summary>
/// Autentica um usuário com e-mail e senha.
/// </summary>
public record ComandoLogin(
    string Email,
    string Senha,
    string? Dispositivo = null) : IRequest<RespostaLogin>;
