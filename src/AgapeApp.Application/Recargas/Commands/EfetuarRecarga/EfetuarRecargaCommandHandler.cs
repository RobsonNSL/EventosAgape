using AgapeApp.Application.Auditoria;
using AgapeApp.Application.Interfaces;
using AgapeApp.Application.Operacoes;
using AgapeApp.Application.Recargas.DTOs;
using AgapeApp.Domain.Entities;
using AgapeApp.Domain.Resources;
using MediatR;

namespace AgapeApp.Application.Recargas.Commands.EfetuarRecarga;

public class EfetuarRecargaCommandHandler : IRequestHandler<EfetuarRecargaCommand, RecargaResponse>
{
    private readonly IRecargaRepository _recargaRepository;
    private readonly ICartaoRepository _cartaoRepository;
    private readonly ISaldoCartaoEventoRepository _saldoCartaoEventoRepository;
    private readonly OperacaoContextoService _operacaoContextoService;
    private readonly AuditoriaService _auditoriaService;
    private readonly INotificadorDashboard _notificadorDashboard;
    private readonly IUnitOfWork _unitOfWork;

    public EfetuarRecargaCommandHandler(
        IRecargaRepository recargaRepository,
        ICartaoRepository cartaoRepository,
        ISaldoCartaoEventoRepository saldoCartaoEventoRepository,
        OperacaoContextoService operacaoContextoService,
        AuditoriaService auditoriaService,
        INotificadorDashboard notificadorDashboard,
        IUnitOfWork unitOfWork)
    {
        _recargaRepository = recargaRepository;
        _cartaoRepository = cartaoRepository;
        _saldoCartaoEventoRepository = saldoCartaoEventoRepository;
        _operacaoContextoService = operacaoContextoService;
        _auditoriaService = auditoriaService;
        _notificadorDashboard = notificadorDashboard;
        _unitOfWork = unitOfWork;
    }

    public async Task<RecargaResponse> Handle(
        EfetuarRecargaCommand comando,
        CancellationToken cancellationToken)
    {
        // Idempotência: retry ou toque duplo do app devolve a recarga já registrada.
        var recargaExistente = await _recargaRepository
            .ObterPorIdOperacaoAsync(comando.IdOperacaoCliente, cancellationToken);

        if (recargaExistente is not null)
            return await MontarRespostaAsync(recargaExistente, cancellationToken);

        // Validações comuns às operações (cartão ativo, evento aberto, setor, perfil e designação).
        var contexto = await _operacaoContextoService.ValidarAsync(
            comando.CartaoCodigo,
            comando.EventoId,
            comando.SetorId,
            comando.UsuarioId,
            cancellationToken);

        var saldo = await _saldoCartaoEventoRepository
            .ObterAsync(contexto.Cartao.Id, contexto.Evento.Id, cancellationToken);

        if (saldo is null)
        {
            saldo = SaldoCartaoEvento.Criar(contexto.Cartao.Id, contexto.Evento.Id);
            _saldoCartaoEventoRepository.Adicionar(saldo);
        }

        saldo.Creditar(comando.Valor);

        var recarga = Recarga.Criar(
            contexto.Cartao.Id,
            contexto.Evento.Id,
            contexto.Setor.Id,
            contexto.Usuario.Id,
            comando.Valor,
            comando.IdOperacaoCliente,
            comando.Dispositivo);

        _recargaRepository.Adicionar(recarga);

        _auditoriaService.Registrar(
            contexto.Usuario.Id,
            "Recarga",
            "Recarga",
            recarga.Id,
            contexto.Evento.Id,
            new { comando.Valor, cartao = contexto.Cartao.Codigo, setor = contexto.Setor.Nome },
            comando.Dispositivo);

        await _unitOfWork.SalvarAlteracoesAsync(cancellationToken);

        var resposta = new RecargaResponse(
            recarga.Id,
            contexto.Cartao.Codigo,
            contexto.Evento.Id,
            recarga.Valor,
            saldo.Saldo,
            recarga.CriadoEm);

        await _notificadorDashboard.NotificarAsync(
            contexto.Evento.Id,
            "Recarga",
            resposta,
            cancellationToken);

        return resposta;
    }

    private async Task<RecargaResponse> MontarRespostaAsync(
        Recarga recarga,
        CancellationToken cancellationToken)
    {
        var cartao = await _cartaoRepository.ObterPorIdAsync(recarga.CartaoId, cancellationToken);

        var saldo = await _saldoCartaoEventoRepository
            .ObterAsync(recarga.CartaoId, recarga.EventoId, cancellationToken);

        return new RecargaResponse(
            recarga.Id,
            cartao?.Codigo ?? string.Empty,
            recarga.EventoId,
            recarga.Valor,
            saldo?.Saldo ?? 0m,
            recarga.CriadoEm);
    }
}
