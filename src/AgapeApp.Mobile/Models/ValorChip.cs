using AgapeApp.Mobile.Helpers;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AgapeApp.Mobile.Models;

/// <summary>Chip de valor rápido da recarga (R$ 5, R$ 10, ...).</summary>
public partial class ValorChip : ObservableObject
{
    public required decimal Valor { get; init; }
    public required string Titulo { get; init; }

    [ObservableProperty]
    private bool selecionado;

    public static ValorChip De(decimal valor) => new()
    {
        Valor = valor,
        Titulo = Formatos.Moeda(valor)
    };
}
