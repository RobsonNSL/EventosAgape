using AgapeApp.Mobile.Helpers;

namespace AgapeApp.Mobile.Models;

/// <summary>Cartão com saldo em circulação no evento (relatório do administrador).</summary>
public class CartaoSaldoItem
{
    public required string CartaoCodigo { get; init; }

    public required decimal Saldo { get; init; }

    public string TextoSaldo => Formatos.Moeda(Saldo);

    public static CartaoSaldoItem De(SaldoEmCirculacaoDTO dto) => new()
    {
        CartaoCodigo = dto.CartaoCodigo,
        Saldo = dto.Saldo
    };
}
