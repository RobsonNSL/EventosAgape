using System.Text.Json;

namespace AgapeApp.Mobile.Helpers;

/// <summary>Leitura do código do cartão vindo do QR (usado no terminal e no cadastro).</summary>
public static class CodigoCartao
{
    /// <summary>
    /// Alguns cartões podem trazer um JSON no QR ({ "codigo": "..." }); nos demais casos o
    /// próprio texto já é o código. Devolve sempre em maiúsculas, como a API grava.
    /// </summary>
    public static string Normalizar(string? valor)
    {
        var texto = (valor ?? string.Empty).Trim();

        if (texto.StartsWith('{'))
        {
            try
            {
                using var documento = JsonDocument.Parse(texto);
                foreach (var campo in new[] { "codigo", "cartao", "card" })
                {
                    if (documento.RootElement.TryGetProperty(campo, out var propriedade)
                        && propriedade.ValueKind == JsonValueKind.String)
                    {
                        texto = propriedade.GetString()?.Trim() ?? texto;
                        break;
                    }
                }
            }
            catch (JsonException)
            {
                // QR comum, sem JSON: usa o texto como veio.
            }
        }

        return texto.ToUpperInvariant();
    }
}
