using AgapeApp.Domain.Common;
using AgapeApp.Domain.Exceptions;
using AgapeApp.Domain.Resources;

namespace AgapeApp.Domain.Entities;

/// <summary>
/// Setor do evento — divisão onde um ou mais operadores vendem (ex.: Pastel, Bebidas).
/// </summary>
public class Setor : BaseEntity
{
    // Construtor privado exigido pelo Entity Framework.
    private Setor()
    {
    }

    public Guid EventoId { get; private set; }

    public string Nome { get; private set; } = null!;

    public static Setor Criar(Guid eventoId, string nome)
    {
        ValidarNome(nome);

        return new Setor
        {
            EventoId = eventoId,
            Nome = nome.Trim()
        };
    }

    public void Renomear(string nome)
    {
        ValidarNome(nome);

        Nome = nome.Trim();
        MarcarComoAtualizada();
    }

    private static void ValidarNome(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new RegraNegocioException(MensagensErro.NomeObrigatorio);
    }
}
