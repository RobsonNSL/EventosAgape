using AgapeApp.Domain.Resources;
using FluentValidation;

namespace AgapeApp.Application.Setores.Commands.CriarSetor;

public class CriarSetorCommandValidator : AbstractValidator<CriarSetorCommand>
{
    public CriarSetorCommandValidator()
    {
        RuleFor(comando => comando.EventoId)
            .NotEmpty().WithMessage(MensagensErro.IdObrigatorio);

        RuleFor(comando => comando.Nome)
            .NotEmpty().WithMessage(MensagensErro.NomeObrigatorio)
            .MaximumLength(100).WithMessage(MensagensErro.NomeTamanhoMaximo);
    }
}
