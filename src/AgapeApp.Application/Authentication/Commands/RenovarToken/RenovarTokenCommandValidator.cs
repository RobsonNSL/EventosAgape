using AgapeApp.Domain.Resources;
using FluentValidation;

namespace AgapeApp.Application.Authentication.Commands.RenovarToken;

public class RenovarTokenCommandValidator : AbstractValidator<RenovarTokenCommand>
{
    public RenovarTokenCommandValidator()
    {
        RuleFor(comando => comando.TokenRenovacao)
            .NotEmpty().WithMessage(MensagensErro.TokenRenovacaoObrigatorio);
    }
}
