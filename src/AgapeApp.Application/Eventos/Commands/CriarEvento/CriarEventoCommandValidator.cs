using AgapeApp.Domain.Resources;
using FluentValidation;

namespace AgapeApp.Application.Eventos.Commands.CriarEvento;

public class CriarEventoCommandValidator : AbstractValidator<CriarEventoCommand>
{
    public CriarEventoCommandValidator()
    {
        RuleFor(comando => comando.Nome)
            .NotEmpty().WithMessage(MensagensErro.NomeObrigatorio)
            .MaximumLength(150).WithMessage(MensagensErro.NomeTamanhoMaximo);

        RuleFor(comando => comando.DataInicio)
            .Must(data => data != default).WithMessage(MensagensErro.DataInicioObrigatoria);

        RuleFor(comando => comando.DataFim)
            .GreaterThanOrEqualTo(comando => comando.DataInicio)
            .WithMessage(MensagensErro.DataFimAnteriorInicio)
            .When(comando => comando.DataFim.HasValue);

        RuleFor(comando => comando.CriadoPor)
            .NotEmpty().WithMessage(MensagensErro.IdObrigatorio);
    }
}
