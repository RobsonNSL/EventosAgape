using AgapeApp.Mobile.Helpers;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AgapeApp.Mobile.Models;

/// <summary>Produto do setor com a quantidade escolhida na venda.</summary>
public partial class ItemCarrinho : ObservableObject, IComImagem
{
    public required Guid EventoProdutoId { get; init; }
    public required string Nome { get; init; }
    public required decimal Preco { get; init; }
    public required bool ControlaEstoque { get; init; }
    public required int Estoque { get; init; }

    /// <summary>Caminho da imagem na API (ex.: "imagens/produtos/coxinha.jpg").</summary>
    public string? CaminhoImagem { get; init; }

    /// <summary>Imagem baixada depois da lista (até lá aparece o ícone).</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TemImagem), nameof(SemImagem))]
    private ImageSource? imagem;

    public bool TemImagem => Imagem is not null;

    public bool SemImagem => Imagem is null;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Subtotal))]
    [NotifyPropertyChangedFor(nameof(TemQuantidade))]
    [NotifyPropertyChangedFor(nameof(AumentaHabilitado))]
    [NotifyPropertyChangedFor(nameof(TextoQuantidade))]
    private int quantidade;

    public decimal Subtotal => Preco * Quantidade;
    public bool TemQuantidade => Quantidade > 0;
    public bool AumentaHabilitado => !ControlaEstoque || Quantidade < Estoque;
    public string TextoQuantidade => Quantidade.ToString();
    public string TextoPreco => ControlaEstoque
        ? $"{Formatos.Moeda(Preco)} · {Estoque} em estoque"
        : $"{Formatos.Moeda(Preco)} · sempre disponível";

    public static ItemCarrinho De(EventoProdutoDTO dto) => new()
    {
        EventoProdutoId = dto.Id,
        Nome = dto.ProdutoNome,
        Preco = dto.Preco,
        ControlaEstoque = dto.ControlaEstoque,
        Estoque = dto.QuantidadeEstoque,
        CaminhoImagem = dto.ProdutoCaminhoImagem
    };
}
