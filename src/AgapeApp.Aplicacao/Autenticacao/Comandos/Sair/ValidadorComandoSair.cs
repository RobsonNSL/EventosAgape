using FluentValidation;

namespace AgapeApp.Aplicacao.Autenticacao.Comandos.Sair;

public class ValidadorComandoSair : AbstractValidator<ComandoSair>
{
    public ValidadorComandoSair()
    {
        RuleFor(comando => comando.TokenRenovacao)
            .NotEmpty().WithMessage("O token de renovação é obrigatório.");
    }
}
