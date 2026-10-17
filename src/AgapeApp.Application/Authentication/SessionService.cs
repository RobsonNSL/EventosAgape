using AgapeApp.Application.Authentication.DTOs;
using AgapeApp.Application.Interfaces;
using AgapeApp.Domain.Entities;

namespace AgapeApp.Application.Authentication;

/// <summary>
/// Emite uma sessão completa (token de acesso + token de renovação) para um usuário.
/// </summary>
public class SessionService
{
    private readonly ITokenService _tokenService;
    private readonly ITokenRenovacaoRepository _tokenRenovacaoRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SessionService(
        ITokenService tokenService,
        ITokenRenovacaoRepository tokenRenovacaoRepository,
        IUnitOfWork unitOfWork)
    {
        _tokenService = tokenService;
        _tokenRenovacaoRepository = tokenRenovacaoRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<LoginResponse> EmitirAsync(Usuario usuario, string? dispositivo, CancellationToken cancellationToken)
    {
        var (tokenAcesso, expiraEm) = _tokenService.GerarTokenAcesso(usuario);

        var tokenRenovacao = _tokenService.GerarTokenRenovacao();

        var registro = TokenRenovacao.Criar(
            usuario.Id,
            _tokenService.CalcularHashTokenRenovacao(tokenRenovacao),
            _tokenService.ObterExpiracaoTokenRenovacao(),
            dispositivo);

        _tokenRenovacaoRepository.Adicionar(registro);
        await _unitOfWork.SalvarAlteracoesAsync(cancellationToken);

        return new LoginResponse(
            tokenAcesso,
            tokenRenovacao,
            expiraEm,
            new UsuarioAutenticadoDTO(usuario.Id, usuario.Nome, usuario.Email, usuario.Perfil.ToString()));
    }
}
