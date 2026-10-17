using AgapeApp.Domain.Resources;
using FluentValidation;

namespace AgapeApp.Application.Eventos.Commands.AtualizarEvento;

public class AtualizarEventoCommandValidator : AbstractValidator<AtualizarEventoCommand>
{
    public AtualizarEventoCommandValidator()
    {
        RuleFor(comando => comando.Id)
            .NotEmpty().WithMessage(MensagensErro.IdObrigatorio);

        RuleFor(comando => comando.Nome)
            .NotEmpty().WithMessage(MensagensErro.NomeObrigatorio)
            .MaximumLength(150).WithMessage(MensagensErro.NomeTamanhoMaximo);

        RuleFor(comando => comando.DataInicio)
            .Must(data => data != default).WithMessage(MensagensErro.DataInicioObrigatoria);

        RuleFor(comando => comando.DataFim)
            .GreaterThanOrEqualTo(comando => comando.DataInicio)
            .WithMessage(MensagensErro.DataFimAnteriorInicio)
            .When(comando => comando.DataFim.HasValue);
    }
}
