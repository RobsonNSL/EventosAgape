using AgapeApp.Domain.Resources;
using FluentValidation;

namespace AgapeApp.Application.Produtos.Commands.AtualizarProduto;

public class AtualizarProdutoCommandValidator : AbstractValidator<AtualizarProdutoCommand>
{
    public AtualizarProdutoCommandValidator()
    {
        RuleFor(comando => comando.Id)
            .NotEmpty().WithMessage(MensagensErro.IdObrigatorio);

        RuleFor(comando => comando.Nome)
            .NotEmpty().WithMessage(MensagensErro.NomeObrigatorio)
            .MaximumLength(150).WithMessage(MensagensErro.NomeTamanhoMaximo);

        RuleFor(comando => comando.PrecoPadrao)
            .GreaterThan(0).WithMessage(MensagensErro.PrecoInvalido)
            .When(comando => comando.PrecoPadrao.HasValue);
    }
}
