using AgapeApp.Domain.Common;

namespace AgapeApp.Domain.Entities;

/// <summary>
/// Vínculo setor × produto do evento: define o que cada setor vende.
/// </summary>
public class SetorProduto : BaseEntity
{
    // Construtor privado exigido pelo Entity Framework.
    private SetorProduto()
    {
    }

    public Guid SetorId { get; private set; }

    public Guid EventoProdutoId { get; private set; }

    public static SetorProduto Criar(Guid setorId, Guid eventoProdutoId) => new()
    {
        SetorId = setorId,
        EventoProdutoId = eventoProdutoId
    };
}
