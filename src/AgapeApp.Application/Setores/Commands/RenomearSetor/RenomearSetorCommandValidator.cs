using AgapeApp.Domain.Resources;
using FluentValidation;

namespace AgapeApp.Application.Setores.Commands.RenomearSetor;

public class RenomearSetorCommandValidator : AbstractValidator<RenomearSetorCommand>
{
    public RenomearSetorCommandValidator()
    {
        RuleFor(comando => comando.Id)
            .NotEmpty().WithMessage(MensagensErro.IdObrigatorio);

        RuleFor(comando => comando.Nome)
            .NotEmpty().WithMessage(MensagensErro.NomeObrigatorio)
            .MaximumLength(100).WithMessage(MensagensErro.NomeTamanhoMaximo);
    }
}
