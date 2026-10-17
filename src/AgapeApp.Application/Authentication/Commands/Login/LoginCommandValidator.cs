using AgapeApp.Domain.Resources;
using FluentValidation;

namespace AgapeApp.Application.Authentication.Commands.Login;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(comando => comando.Email)
            .NotEmpty().WithMessage(MensagensErro.EmailObrigatorio)
            .EmailAddress().WithMessage(MensagensErro.EmailInvalido);

        RuleFor(comando => comando.Senha)
            .NotEmpty().WithMessage(MensagensErro.SenhaObrigatoria);
    }
}
