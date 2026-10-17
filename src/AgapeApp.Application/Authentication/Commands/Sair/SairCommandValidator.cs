using AgapeApp.Domain.Resources;
using FluentValidation;

namespace AgapeApp.Application.Authentication.Commands.Sair;

public class SairCommandValidator : AbstractValidator<SairCommand>
{
    public SairCommandValidator()
    {
        RuleFor(comando => comando.TokenRenovacao)
            .NotEmpty().WithMessage(MensagensErro.TokenRenovacaoObrigatorio);
    }
}
