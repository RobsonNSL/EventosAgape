using System.Reflection;
using AgapeApp.Domain.Resources;

namespace AgapeApp.Domain.Tests;

public class MensagensErroTests
{
    [Fact]
    public void TodasAsMensagens_DevemExistirNoArquivoDeResources()
    {
        var propriedades = typeof(MensagensErro)
            .GetProperties(BindingFlags.Public | BindingFlags.Static)
            .Where(propriedade => propriedade.PropertyType == typeof(string))
            .ToList();

        Assert.NotEmpty(propriedades);

        foreach (var propriedade in propriedades)
        {
            var valor = (string?)propriedade.GetValue(null);

            // Sem chave no .resx, o acessor devolve o próprio nome da propriedade.
            Assert.False(
                string.IsNullOrWhiteSpace(valor),
                $"A mensagem '{propriedade.Name}' está vazia.");

            Assert.NotEqual(propriedade.Name, valor);
        }
    }
}
