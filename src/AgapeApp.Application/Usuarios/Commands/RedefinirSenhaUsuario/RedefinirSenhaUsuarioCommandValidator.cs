using AgapeApp.Domain.Resources;
using FluentValidation;

namespace AgapeApp.Application.Usuarios.Commands.RedefinirSenhaUsuario;

public class RedefinirSenhaUsuarioCommandValidator : AbstractValidator<RedefinirSenhaUsuarioCommand>
{
    public RedefinirSenhaUsuarioCommandValidator()
    {
        RuleFor(comando => comando.Id)
            .NotEmpty().WithMessage(MensagensErro.IdObrigatorio);

        RuleFor(comando => comando.NovaSenha)
            .NotEmpty().WithMessage(MensagensErro.SenhaObrigatoria)
            .MinimumLength(8).WithMessage(MensagensErro.SenhaTamanhoMinimo);
    }
}
