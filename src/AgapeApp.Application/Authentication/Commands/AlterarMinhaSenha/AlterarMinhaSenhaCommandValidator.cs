using AgapeApp.Domain.Resources;
using FluentValidation;

namespace AgapeApp.Application.Authentication.Commands.AlterarMinhaSenha;

public class AlterarMinhaSenhaCommandValidator : AbstractValidator<AlterarMinhaSenhaCommand>
{
    public AlterarMinhaSenhaCommandValidator()
    {
        RuleFor(comando => comando.UsuarioId)
            .NotEmpty().WithMessage(MensagensErro.IdObrigatorio);

        RuleFor(comando => comando.SenhaAtual)
            .NotEmpty().WithMessage(MensagensErro.SenhaObrigatoria);

        RuleFor(comando => comando.NovaSenha)
            .NotEmpty().WithMessage(MensagensErro.SenhaObrigatoria)
            .MinimumLength(8).WithMessage(MensagensErro.SenhaTamanhoMinimo);
    }
}
