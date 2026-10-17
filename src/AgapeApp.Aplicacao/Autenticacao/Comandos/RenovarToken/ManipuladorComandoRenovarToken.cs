using AgapeApp.Aplicacao.Autenticacao.Dtos;
using AgapeApp.Aplicacao.Comum.Excecoes;
using AgapeApp.Aplicacao.Interfaces;
using MediatR;

namespace AgapeApp.Aplicacao.Autenticacao.Comandos.RenovarToken;

public class ManipuladorComandoRenovarToken : IRequestHandler<ComandoRenovarToken, RespostaLogin>
{
    private readonly IRepositorioTokensRenovacao _repositorioTokensRenovacao;
    private readonly IRepositorioUsuarios _repositorioUsuarios;
    private readonly IServicoToken _servicoToken;
    private readonly EmissorDeSessao _emissorDeSessao;

    public ManipuladorComandoRenovarToken(
        IRepositorioTokensRenovacao repositorioTokensRenovacao,
        IRepositorioUsuarios repositorioUsuarios,
        IServicoToken servicoToken,
        EmissorDeSessao emissorDeSessao)
    {
        _repositorioTokensRenovacao = repositorioTokensRenovacao;
        _repositorioUsuarios = repositorioUsuarios;
        _servicoToken = servicoToken;
        _emissorDeSessao = emissorDeSessao;
    }

    public async Task<RespostaLogin> Handle(ComandoRenovarToken comando, CancellationToken cancellationToken)
    {
        var hash = _servicoToken.CalcularHashTokenRenovacao(comando.TokenRenovacao);

        var registro = await _repositorioTokensRenovacao.ObterPorHashAsync(hash, cancellationToken)
            ?? throw new ExcecaoAutenticacao("Sessão inválida. Faça login novamente.");

        if (!registro.EstaValido(DateTimeOffset.UtcNow))
            throw new ExcecaoAutenticacao("Sessão expirada. Faça login novamente.");

        var usuario = await _repositorioUsuarios.ObterPorIdAsync(registro.UsuarioId, cancellationToken)
            ?? throw new ExcecaoAutenticacao("Sessão inválida. Faça login novamente.");

        if (!usuario.Ativo)
            throw new ExcecaoAutenticacao("Este usuário está desativado. Procure um administrador.");

        // Rotação: o token antigo é revogado e um novo par é emitido.
        registro.Revogar();

        return await _emissorDeSessao.EmitirAsync(usuario, comando.Dispositivo, cancellationToken);
    }
}
