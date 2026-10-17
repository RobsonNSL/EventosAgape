using AgapeApp.Domain.Resources;
using FluentValidation;

namespace AgapeApp.Application.Recargas.Commands.EfetuarRecarga;

public class EfetuarRecargaCommandValidator : AbstractValidator<EfetuarRecargaCommand>
{
    public EfetuarRecargaCommandValidator()
    {
        RuleFor(comando => comando.CartaoCodigo)
            .NotEmpty().WithMessage(MensagensErro.CartaoCodigoObrigatorio)
            .MaximumLength(50).WithMessage(MensagensErro.CartaoCodigoTamanhoMaximo);

        RuleFor(comando => comando.EventoId)
            .NotEmpty().WithMessage(MensagensErro.IdObrigatorio);

        RuleFor(comando => comando.SetorId)
            .NotEmpty().WithMessage(MensagensErro.IdObrigatorio);

        RuleFor(comando => comando.Valor)
            .GreaterThan(0).WithMessage(MensagensErro.ValorInvalido);

        RuleFor(comando => comando.IdOperacaoCliente)
            .NotEmpty().WithMessage(MensagensErro.IdOperacaoObrigatoria);

        RuleFor(comando => comando.Dispositivo)
            .MaximumLength(100).WithMessage(MensagensErro.DispositivoTamanhoMaximo);

        RuleFor(comando => comando.UsuarioId)
            .NotEmpty().WithMessage(MensagensErro.IdObrigatorio);
    }
}
