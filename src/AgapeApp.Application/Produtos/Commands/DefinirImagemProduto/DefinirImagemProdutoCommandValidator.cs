using AgapeApp.Application.Common;
using AgapeApp.Domain.Resources;
using FluentValidation;

namespace AgapeApp.Application.Produtos.Commands.DefinirImagemProduto;

/// <summary>Mesmas regras da foto de perfil: jpg/jpeg/png/webp até 5 MB.</summary>
public class DefinirImagemProdutoCommandValidator : AbstractValidator<DefinirImagemProdutoCommand>
{
    public DefinirImagemProdutoCommandValidator()
    {
        RuleFor(comando => comando.ProdutoId)
            .NotEmpty().WithMessage(MensagensErro.IdObrigatorio);

        RuleFor(comando => comando.Conteudo)
            .NotNull().WithMessage(MensagensErro.FotoObrigatoria);

        RuleFor(comando => comando.NomeArquivo)
            .Must(FotoPerfil.ExtensaoPermitida).WithMessage(MensagensErro.FotoFormatoInvalido);

        RuleFor(comando => comando.Tamanho)
            .GreaterThan(0).WithMessage(MensagensErro.FotoArquivoVazio)
            .LessThanOrEqualTo(FotoPerfil.TamanhoMaximoBytes).WithMessage(MensagensErro.FotoMuitoGrande);
    }
}
