namespace AgapeApp.Mobile.Helpers;

/// <summary>Leitura dos parâmetros de navegação do Shell (query string ou objetos).</summary>
public static class Parametros
{
    public static string Texto(IDictionary<string, object> query, string chave)
    {
        if (!query.TryGetValue(chave, out var valor) || valor is null)
            return string.Empty;

        var texto = valor.ToString() ?? string.Empty;
        return texto.Contains('%') ? Uri.UnescapeDataString(texto) : texto;
    }

    public static Guid Guid(IDictionary<string, object> query, string chave)
        => query.TryGetValue(chave, out var valor) && valor is Guid guid
            ? guid
            : System.Guid.TryParse(Texto(query, chave), out var convertido) ? convertido : System.Guid.Empty;

    public static T? Objeto<T>(IDictionary<string, object> query, string chave) where T : class
        => query.TryGetValue(chave, out var valor) && valor is T objeto ? objeto : null;
}
