using AgapeApp.Domain.Resources;
using FluentValidation;

namespace AgapeApp.Application.Vendas.Commands.RegistrarVenda;

public class RegistrarVendaCommandValidator : AbstractValidator<RegistrarVendaCommand>
{
    public RegistrarVendaCommandValidator()
    {
        RuleFor(comando => comando.CartaoCodigo)
            .NotEmpty().WithMessage(MensagensErro.CartaoCodigoObrigatorio)
            .MaximumLength(50).WithMessage(MensagensErro.CartaoCodigoTamanhoMaximo);

        RuleFor(comando => comando.EventoId)
            .NotEmpty().WithMessage(MensagensErro.IdObrigatorio);

        RuleFor(comando => comando.SetorId)
            .NotEmpty().WithMessage(MensagensErro.IdObrigatorio);

        RuleFor(comando => comando.Itens)
            .NotEmpty().WithMessage(MensagensErro.VendaSemItens);

        RuleForEach(comando => comando.Itens).ChildRules(item =>
        {
            item.RuleFor(i => i.EventoProdutoId)
                .NotEmpty().WithMessage(MensagensErro.IdObrigatorio);

            item.RuleFor(i => i.Quantidade)
                .GreaterThan(0).WithMessage(MensagensErro.QuantidadeInvalida);
        });

        RuleFor(comando => comando.IdOperacaoCliente)
            .NotEmpty().WithMessage(MensagensErro.IdOperacaoObrigatoria);

        RuleFor(comando => comando.Dispositivo)
            .MaximumLength(100).WithMessage(MensagensErro.DispositivoTamanhoMaximo);

        RuleFor(comando => comando.UsuarioId)
            .NotEmpty().WithMessage(MensagensErro.IdObrigatorio);
    }
}
