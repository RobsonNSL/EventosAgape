using System.Globalization;

namespace AgapeApp.Mobile.Helpers;

/// <summary>Formatação em português (moeda, datas e iniciais) para a interface.</summary>
public static class Formatos
{
    public static CultureInfo Cultura { get; } = CriarCultura();

    private static CultureInfo CriarCultura()
    {
        try
        {
            return CultureInfo.GetCultureInfo("pt-BR");
        }
        catch (CultureNotFoundException)
        {
            return CultureInfo.InvariantCulture;
        }
    }

    public static string Moeda(decimal valor) => valor.ToString("C", Cultura);

    public static string DataEvento(DateOnly inicio, DateOnly? fim)
    {
        var textoInicio = Dia(inicio);
        if (fim is null || fim.Value == inicio)
            return textoInicio;
        return $"{textoInicio} – {Dia(fim.Value)}";
    }

    private static string Dia(DateOnly data)
    {
        var texto = data.ToDateTime(TimeOnly.MinValue).ToString("ddd, dd/MM", Cultura);
        return string.IsNullOrEmpty(texto) ? data.ToString("dd/MM") : char.ToUpper(texto[0], Cultura) + texto[1..];
    }

    public static string Iniciais(string? nome)
    {
        var partes = (nome ?? string.Empty).Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (partes.Length == 0)
            return "?";
        var texto = partes.Length == 1
            ? partes[0][..1]
            : $"{partes[0][0]}{partes[^1][0]}";
        return texto.ToUpper(Cultura);
    }

    /// <summary>Busca de texto ignorando maiúsculas e acentos (ex.: "catequese" acha "Catequese").</summary>
    public static bool Contem(string? texto, string? busca)
        => string.IsNullOrWhiteSpace(busca)
           || (!string.IsNullOrEmpty(texto) && Cultura.CompareInfo.IndexOf(texto, busca, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) >= 0);

    /// <summary>Converte texto digitado ("12,50" ou "12.50" ou "R$ 12,50") em valor.</summary>
    public static bool TentarValor(string? texto, out decimal valor)
    {
        valor = 0;
        if (string.IsNullOrWhiteSpace(texto))
            return false;

        var limpo = texto.Replace("R$", string.Empty, StringComparison.OrdinalIgnoreCase).Trim();
        return decimal.TryParse(limpo, NumberStyles.Number, Cultura, out valor)
               || decimal.TryParse(limpo.Replace(',', '.'), NumberStyles.Number, CultureInfo.InvariantCulture, out valor);
    }
}
