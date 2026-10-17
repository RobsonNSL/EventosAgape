using AgapeApp.Domain.Resources;
using FluentValidation;

namespace AgapeApp.Application.Usuarios.Commands.AlterarAtivacaoUsuario;

public class AlterarAtivacaoUsuarioCommandValidator : AbstractValidator<AlterarAtivacaoUsuarioCommand>
{
    public AlterarAtivacaoUsuarioCommandValidator()
    {
        RuleFor(comando => comando.Id)
            .NotEmpty().WithMessage(MensagensErro.IdObrigatorio);

        RuleFor(comando => comando.SolicitanteId)
            .NotEmpty().WithMessage(MensagensErro.IdObrigatorio);
    }
}
