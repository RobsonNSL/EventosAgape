using AgapeApp.Domain.Resources;
using FluentValidation;

namespace AgapeApp.Application.Cartoes.Commands.AlterarAtivacaoCartao;

public class AlterarAtivacaoCartaoCommandValidator : AbstractValidator<AlterarAtivacaoCartaoCommand>
{
    public AlterarAtivacaoCartaoCommandValidator()
    {
        RuleFor(comando => comando.Id)
            .NotEmpty().WithMessage(MensagensErro.IdObrigatorio);
    }
}
