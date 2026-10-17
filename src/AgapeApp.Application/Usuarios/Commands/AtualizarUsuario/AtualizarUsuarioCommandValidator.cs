using AgapeApp.Domain.Resources;
using FluentValidation;

namespace AgapeApp.Application.Usuarios.Commands.AtualizarUsuario;

public class AtualizarUsuarioCommandValidator : AbstractValidator<AtualizarUsuarioCommand>
{
    public AtualizarUsuarioCommandValidator()
    {
        RuleFor(comando => comando.Id)
            .NotEmpty().WithMessage(MensagensErro.IdObrigatorio);

        RuleFor(comando => comando.Nome)
            .NotEmpty().WithMessage(MensagensErro.NomeObrigatorio)
            .MaximumLength(150).WithMessage(MensagensErro.NomeTamanhoMaximo);

        RuleFor(comando => comando.Email)
            .NotEmpty().WithMessage(MensagensErro.EmailObrigatorio)
            .EmailAddress().WithMessage(MensagensErro.EmailInvalido)
            .MaximumLength(200).WithMessage(MensagensErro.EmailTamanhoMaximo);

        RuleFor(comando => comando.Perfil)
            .IsInEnum().WithMessage(MensagensErro.PerfilInvalido);
    }
}
