namespace AgapeApp.Mobile.Models;

/// <summary>
/// Operação (recarga ou venda) feita neste aparelho. Fica no cache local
/// para o operador conferir o que já passou, mesmo sem internet.
/// </summary>
public class OperacaoRecente
{
    public required string Tipo { get; init; }
    public required string CartaoCodigo { get; init; }
    public required string Detalhe { get; init; }
    public required string ValorTexto { get; init; }
    public required string HoraTexto { get; init; }
    public required string Icone { get; init; }
    public required Color Cor { get; init; }
    public required Color Fundo { get; init; }

    public bool EhRecarga => Tipo == "Recarga";
}
