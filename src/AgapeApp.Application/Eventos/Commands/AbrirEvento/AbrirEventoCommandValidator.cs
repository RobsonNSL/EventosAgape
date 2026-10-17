using AgapeApp.Domain.Resources;
using FluentValidation;

namespace AgapeApp.Application.Eventos.Commands.AbrirEvento;

public class AbrirEventoCommandValidator : AbstractValidator<AbrirEventoCommand>
{
    public AbrirEventoCommandValidator()
    {
        RuleFor(comando => comando.Id)
            .NotEmpty().WithMessage(MensagensErro.IdObrigatorio);

        RuleFor(comando => comando.UsuarioId)
            .NotEmpty().WithMessage(MensagensErro.IdObrigatorio);
    }
}
