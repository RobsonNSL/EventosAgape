using AgapeApp.Domain.Resources;
using FluentValidation;

namespace AgapeApp.Application.EventoProdutos.Commands.VincularProdutoAoEvento;

public class VincularProdutoAoEventoCommandValidator : AbstractValidator<VincularProdutoAoEventoCommand>
{
    public VincularProdutoAoEventoCommandValidator()
    {
        RuleFor(comando => comando.EventoId)
            .NotEmpty().WithMessage(MensagensErro.IdObrigatorio);

        RuleFor(comando => comando.ProdutoId)
            .NotEmpty().WithMessage(MensagensErro.IdObrigatorio);

        RuleFor(comando => comando.UsuarioId)
            .NotEmpty().WithMessage(MensagensErro.IdObrigatorio);

        RuleFor(comando => comando.Preco)
            .GreaterThan(0).WithMessage(MensagensErro.PrecoInvalido);

        RuleFor(comando => comando.QuantidadeEstoqueInicial)
            .GreaterThanOrEqualTo(0).WithMessage(MensagensErro.QuantidadeEstoqueNegativa);
    }
}
