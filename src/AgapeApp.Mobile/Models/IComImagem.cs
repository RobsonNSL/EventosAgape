namespace AgapeApp.Mobile.Models;

/// <summary>Item de lista que mostra uma imagem vinda da API (produto, usuário...).</summary>
public interface IComImagem
{
    string? CaminhoImagem { get; }

    ImageSource? Imagem { get; set; }
}
