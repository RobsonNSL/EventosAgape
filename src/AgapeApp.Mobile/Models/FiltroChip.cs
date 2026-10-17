using CommunityToolkit.Mvvm.ComponentModel;

namespace AgapeApp.Mobile.Models;

/// <summary>Chip de filtro da lista de eventos (Todos, Abertos, ...).</summary>
public partial class FiltroChip : ObservableObject
{
    public required string Chave { get; init; }
    public required string Titulo { get; init; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Texto))]
    private int contagem;

    [ObservableProperty]
    private bool selecionado;

    public string Texto => Contagem > 0 ? $"{Titulo} · {Contagem}" : Titulo;

    public static FiltroChip De(string chave, string titulo, int contagem, bool selecionado) => new()
    {
        Chave = chave,
        Titulo = titulo,
        Contagem = contagem,
        Selecionado = selecionado
    };
}
