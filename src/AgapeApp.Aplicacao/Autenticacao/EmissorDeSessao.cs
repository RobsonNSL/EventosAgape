using AgapeApp.Aplicacao.Autenticacao.Dtos;
using AgapeApp.Aplicacao.Interfaces;
using AgapeApp.Dominio.Entidades;

namespace AgapeApp.Aplicacao.Autenticacao;

/// <summary>
/// Emite uma sessão completa (token de acesso + token de renovação) para um usuário.
/// </summary>
public class EmissorDeSessao
{
    private readonly IServicoToken _servicoToken;
    private readonly IRepositorioTokensRenovacao _repositorioTokensRenovacao;
    private readonly IUnidadeDeTrabalho _unidadeDeTrabalho;

    public EmissorDeSessao(
        IServicoToken servicoToken,
        IRepositorioTokensRenovacao repositorioTokensRenovacao,
        IUnidadeDeTrabalho unidadeDeTrabalho)
    {
        _servicoToken = servicoToken;
        _repositorioTokensRenovacao = repositorioTokensRenovacao;
        _unidadeDeTrabalho = unidadeDeTrabalho;
    }

    public async Task<RespostaLogin> EmitirAsync(Usuario usuario, string? dispositivo, CancellationToken cancellationToken)
    {
        var (tokenAcesso, expiraEm) = _servicoToken.GerarTokenAcesso(usuario);

        var tokenRenovacao = _servicoToken.GerarTokenRenovacao();

        var registro = TokenRenovacao.Criar(
            usuario.Id,
            _servicoToken.CalcularHashTokenRenovacao(tokenRenovacao),
            _servicoToken.ObterExpiracaoTokenRenovacao(),
            dispositivo);

        _repositorioTokensRenovacao.Adicionar(registro);
        await _unidadeDeTrabalho.SalvarAlteracoesAsync(cancellationToken);

        return new RespostaLogin(
            tokenAcesso,
            tokenRenovacao,
            expiraEm,
            new UsuarioAutenticado(usuario.Id, usuario.Nome, usuario.Email, usuario.Perfil.ToString()));
    }
}
