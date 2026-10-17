using AgapeApp.Application.Common.Exceptions;
using AgapeApp.Application.Interfaces;
using AgapeApp.Domain.Entities;
using AgapeApp.Domain.Exceptions;
using AgapeApp.Domain.Resources;

namespace AgapeApp.Application.Operacoes;

/// <summary>
/// Contexto validado de uma operação de caixa (recarga ou venda).
/// </summary>
public record ContextoOperacao(Cartao Cartao, Evento Evento, Setor Setor, Usuario Usuario);

/// <summary>
/// Valida o contexto comum das operações de caixa:
/// cartão existe e está ativo, evento existe e está aberto, setor pertence ao evento,
/// usuário está ativo e, quando operador, está designado e habilitado no próprio setor.
/// Administrador pode operar em qualquer setor do evento aberto.
/// </summary>
public class OperacaoContextoService
{
    private readonly ICartaoRepository _cartaoRepository;
    private readonly IEventoRepository _eventoRepository;
    private readonly ISetorRepository _setorRepository;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IEventoOperadorRepository _eventoOperadorRepository;

    public OperacaoContextoService(
        ICartaoRepository cartaoRepository,
        IEventoRepository eventoRepository,
        ISetorRepository setorRepository,
        IUsuarioRepository usuarioRepository,
        IEventoOperadorRepository eventoOperadorRepository)
    {
        _cartaoRepository = cartaoRepository;
        _eventoRepository = eventoRepository;
        _setorRepository = setorRepository;
        _usuarioRepository = usuarioRepository;
        _eventoOperadorRepository = eventoOperadorRepository;
    }

    public async Task<ContextoOperacao> ValidarAsync(
        string cartaoCodigo,
        Guid eventoId,
        Guid setorId,
        Guid usuarioId,
        CancellationToken cancellationToken)
    {
        var codigoNormalizado = Cartao.NormalizarCodigo(cartaoCodigo);

        var cartao = await _cartaoRepository.ObterPorCodigoAsync(codigoNormalizado, cancellationToken)
            ?? throw new RegraNegocioException(MensagensErro.CartaoNaoEncontrado);

        if (!cartao.Ativo)
            throw new RegraNegocioException(MensagensErro.CartaoDesativado);

        var evento = await _eventoRepository.ObterPorIdAsync(eventoId, cancellationToken)
            ?? throw new RegraNegocioException(MensagensErro.EventoNaoEncontrado);

        if (!evento.EstaAberto())
            throw new RegraNegocioException(MensagensErro.EventoNaoEstaAberto);

        var setor = await _setorRepository.ObterPorIdAsync(setorId, cancellationToken)
            ?? throw new RegraNegocioException(MensagensErro.SetorNaoEncontrado);

        if (setor.EventoId != evento.Id)
            throw new RegraNegocioException(MensagensErro.SetorDeOutroEvento);

        var usuario = await _usuarioRepository.ObterPorIdAsync(usuarioId, cancellationToken)
            ?? throw new AutenticacaoException(MensagensErro.SessaoInvalida);

        if (!usuario.Ativo)
            throw new AutenticacaoException(MensagensErro.UsuarioDesativado);

        // Operador só opera no evento designado e no próprio setor; admin pode em qualquer setor aberto.
        if (!usuario.EhAdministrador())
        {
            var designacao = await _eventoOperadorRepository
                .ObterPorEventoEUsuarioAsync(evento.Id, usuario.Id, cancellationToken);

            if (designacao is null || !designacao.Habilitado)
                throw new RegraNegocioException(MensagensErro.OperadorSemPermissaoNoEvento);

            if (designacao.SetorId != setor.Id)
                throw new RegraNegocioException(MensagensErro.OperadorDeOutroSetor);
        }

        return new ContextoOperacao(cartao, evento, setor, usuario);
    }
}
