using AgapeApp.Domain.Resources;
using FluentValidation;

namespace AgapeApp.Application.SetorProdutos.Commands.DesvincularProdutoDoSetor;

public class DesvincularProdutoDoSetorCommandValidator
    : AbstractValidator<DesvincularProdutoDoSetorCommand>
{
    public DesvincularProdutoDoSetorCommandValidator()
    {
        RuleFor(comando => comando.SetorId)
            .NotEmpty().WithMessage(MensagensErro.IdObrigatorio);

        RuleFor(comando => comando.EventoProdutoId)
            .NotEmpty().WithMessage(MensagensErro.IdObrigatorio);
    }
}
