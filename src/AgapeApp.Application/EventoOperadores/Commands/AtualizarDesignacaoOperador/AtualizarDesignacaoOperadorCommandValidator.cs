using AgapeApp.Domain.Resources;
using FluentValidation;

namespace AgapeApp.Application.EventoOperadores.Commands.AtualizarDesignacaoOperador;

public class AtualizarDesignacaoOperadorCommandValidator
    : AbstractValidator<AtualizarDesignacaoOperadorCommand>
{
    public AtualizarDesignacaoOperadorCommandValidator()
    {
        RuleFor(comando => comando.Id)
            .NotEmpty().WithMessage(MensagensErro.IdObrigatorio);

        RuleFor(comando => comando.SetorId)
            .NotEmpty().WithMessage(MensagensErro.IdObrigatorio);

        RuleFor(comando => comando.SolicitanteId)
            .NotEmpty().WithMessage(MensagensErro.IdObrigatorio);
    }
}
