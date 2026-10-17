using AgapeApp.Application.Vendas.DTOs;
using MediatR;

namespace AgapeApp.Application.Vendas.Commands.RegistrarVenda;

/// <summary>
/// Registra uma venda no evento aberto em que o operador está trabalhando:
/// valida o vínculo setor × produto, debita o saldo do cartão, baixa o estoque
/// e grava os itens com o preço praticado no momento.
/// O campo IdOperacaoCliente (gerado pelo app) garante a idempotência.
/// O campo UsuarioId é preenchido pela API com o usuário autenticado.
/// </summary>
public record RegistrarVendaCommand(
    string CartaoCodigo,
    Guid EventoId,
    Guid SetorId,
    IReadOnlyList<ItemVendaPedido> Itens,
    Guid IdOperacaoCliente,
    string? Dispositivo,
    Guid UsuarioId) : IRequest<VendaResponse>;
