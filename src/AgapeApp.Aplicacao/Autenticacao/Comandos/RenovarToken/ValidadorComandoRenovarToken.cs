using FluentValidation;

namespace AgapeApp.Aplicacao.Autenticacao.Comandos.RenovarToken;

public class ValidadorComandoRenovarToken : AbstractValidator<ComandoRenovarToken>
{
    public ValidadorComandoRenovarToken()
    {
        RuleFor(comando => comando.TokenRenovacao)
            .NotEmpty().WithMessage("O token de renovação é obrigatório.");
    }
}
