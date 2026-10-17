using AgapeApp.Domain.Common;
using AgapeApp.Domain.Exceptions;
using AgapeApp.Domain.Resources;

namespace AgapeApp.Domain.Entities;

/// <summary>
/// Cartão pré-pago ao portador, identificado por um código único (ex.: CARD-0001)
/// lido via QR code ou digitado manualmente.
/// </summary>
public class Cartao : BaseEntity
{
    // Construtor privado exigido pelo Entity Framework.
    private Cartao()
    {
    }

    public string Codigo { get; private set; } = null!;

    public bool Ativo { get; private set; }

    /// <summary>
    /// Cria um novo cartão ativo.
    /// </summary>
    public static Cartao Criar(string codigo)
    {
        if (string.IsNullOrWhiteSpace(codigo))
            throw new RegraNegocioException(MensagensErro.CartaoCodigoObrigatorio);

        return new Cartao
        {
            Codigo = NormalizarCodigo(codigo),
            Ativo = true
        };
    }

    /// <summary>
    /// Normaliza o código para comparação e armazenamento (sem espaços e em maiúsculas).
    /// </summary>
    public static string NormalizarCodigo(string codigo) => codigo.Trim().ToUpperInvariant();

    public void Ativar()
    {
        Ativo = true;
        MarcarComoAtualizada();
    }

    public void Desativar()
    {
        Ativo = false;
        MarcarComoAtualizada();
    }
}
