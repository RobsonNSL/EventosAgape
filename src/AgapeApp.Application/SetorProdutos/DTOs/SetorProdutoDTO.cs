namespace AgapeApp.Application.SetorProdutos.DTOs;

/// <summary>
/// Vínculo entre um setor e um produto do evento.
/// </summary>
public record SetorProdutoDTO(
    Guid Id,
    Guid SetorId,
    Guid EventoProdutoId);
