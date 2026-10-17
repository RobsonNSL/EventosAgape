using AgapeApp.Domain.Resources;
using FluentValidation;

namespace AgapeApp.Application.Eventos.Commands.EncerrarEvento;

public class EncerrarEventoCommandValidator : AbstractValidator<EncerrarEventoCommand>
{
    public EncerrarEventoCommandValidator()
    {
        RuleFor(comando => comando.Id)
            .NotEmpty().WithMessage(MensagensErro.IdObrigatorio);

        RuleFor(comando => comando.UsuarioId)
            .NotEmpty().WithMessage(MensagensErro.IdObrigatorio);
    }
}
