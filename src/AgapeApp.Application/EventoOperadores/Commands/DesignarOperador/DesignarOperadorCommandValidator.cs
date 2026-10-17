using AgapeApp.Domain.Resources;
using FluentValidation;

namespace AgapeApp.Application.EventoOperadores.Commands.DesignarOperador;

public class DesignarOperadorCommandValidator : AbstractValidator<DesignarOperadorCommand>
{
    public DesignarOperadorCommandValidator()
    {
        RuleFor(comando => comando.EventoId)
            .NotEmpty().WithMessage(MensagensErro.IdObrigatorio);

        RuleFor(comando => comando.UsuarioId)
            .NotEmpty().WithMessage(MensagensErro.IdObrigatorio);

        RuleFor(comando => comando.SetorId)
            .NotEmpty().WithMessage(MensagensErro.IdObrigatorio);

        RuleFor(comando => comando.SolicitanteId)
            .NotEmpty().WithMessage(MensagensErro.IdObrigatorio);
    }
}
