using AgapeApp.Domain.Resources;
using FluentValidation;

namespace AgapeApp.Application.Cartoes.Commands.CriarCartao;

public class CriarCartaoCommandValidator : AbstractValidator<CriarCartaoCommand>
{
    public CriarCartaoCommandValidator()
    {
        RuleFor(comando => comando.Codigo)
            .NotEmpty().WithMessage(MensagensErro.CartaoCodigoObrigatorio)
            .MaximumLength(50).WithMessage(MensagensErro.CartaoCodigoTamanhoMaximo);
    }
}
