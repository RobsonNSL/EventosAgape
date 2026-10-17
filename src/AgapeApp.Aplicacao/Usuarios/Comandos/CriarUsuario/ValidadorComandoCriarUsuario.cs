using FluentValidation;

namespace AgapeApp.Aplicacao.Usuarios.Comandos.CriarUsuario;

public class ValidadorComandoCriarUsuario : AbstractValidator<ComandoCriarUsuario>
{
    public ValidadorComandoCriarUsuario()
    {
        RuleFor(comando => comando.Nome)
            .NotEmpty().WithMessage("O nome é obrigatório.")
            .MaximumLength(150).WithMessage("O nome deve ter no máximo 150 caracteres.");

        RuleFor(comando => comando.Email)
            .NotEmpty().WithMessage("O e-mail é obrigatório.")
            .EmailAddress().WithMessage("O e-mail informado é inválido.")
            .MaximumLength(200).WithMessage("O e-mail deve ter no máximo 200 caracteres.");

        RuleFor(comando => comando.Senha)
            .NotEmpty().WithMessage("A senha é obrigatória.")
            .MinimumLength(8).WithMessage("A senha deve ter no mínimo 8 caracteres.");

        RuleFor(comando => comando.Perfil)
            .IsInEnum().WithMessage("O perfil informado é inválido.");
    }
}
