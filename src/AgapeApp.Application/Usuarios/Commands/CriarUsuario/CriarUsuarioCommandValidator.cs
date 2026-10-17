using AgapeApp.Domain.Resources;
using FluentValidation;

namespace AgapeApp.Application.Usuarios.Commands.CriarUsuario;

public class CriarUsuarioCommandValidator : AbstractValidator<CriarUsuarioCommand>
{
    public CriarUsuarioCommandValidator()
    {
        RuleFor(comando => comando.Nome)
            .NotEmpty().WithMessage(MensagensErro.NomeObrigatorio)
            .MaximumLength(150).WithMessage(MensagensErro.NomeTamanhoMaximo);

        RuleFor(comando => comando.Email)
            .NotEmpty().WithMessage(MensagensErro.EmailObrigatorio)
            .EmailAddress().WithMessage(MensagensErro.EmailInvalido)
            .MaximumLength(200).WithMessage(MensagensErro.EmailTamanhoMaximo);

        RuleFor(comando => comando.Senha)
            .NotEmpty().WithMessage(MensagensErro.SenhaObrigatoria)
            .MinimumLength(8).WithMessage(MensagensErro.SenhaTamanhoMinimo);

        RuleFor(comando => comando.Perfil)
            .IsInEnum().WithMessage(MensagensErro.PerfilInvalido);
    }
}
