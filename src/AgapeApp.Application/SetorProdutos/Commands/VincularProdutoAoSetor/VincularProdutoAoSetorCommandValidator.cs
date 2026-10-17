using AgapeApp.Domain.Resources;
using FluentValidation;

namespace AgapeApp.Application.SetorProdutos.Commands.VincularProdutoAoSetor;

public class VincularProdutoAoSetorCommandValidator : AbstractValidator<VincularProdutoAoSetorCommand>
{
    public VincularProdutoAoSetorCommandValidator()
    {
        RuleFor(comando => comando.SetorId)
            .NotEmpty().WithMessage(MensagensErro.IdObrigatorio);

        RuleFor(comando => comando.EventoProdutoId)
            .NotEmpty().WithMessage(MensagensErro.IdObrigatorio);
    }
}
