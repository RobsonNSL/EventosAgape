using FluentValidation;

namespace AgapeApp.Aplicacao.Autenticacao.Comandos.Login;

public class ValidadorComandoLogin : AbstractValidator<ComandoLogin>
{
    public ValidadorComandoLogin()
    {
        RuleFor(comando => comando.Email)
            .NotEmpty().WithMessage("O e-mail é obrigatório.")
            .EmailAddress().WithMessage("O e-mail informado é inválido.");

        RuleFor(comando => comando.Senha)
            .NotEmpty().WithMessage("A senha é obrigatória.");
    }
}
