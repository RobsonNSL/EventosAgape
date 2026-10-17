using AgapeApp.Domain.Resources;
using FluentValidation;

namespace AgapeApp.Application.EventoProdutos.Commands.AtualizarEventoProduto;

public class AtualizarEventoProdutoCommandValidator : AbstractValidator<AtualizarEventoProdutoCommand>
{
    public AtualizarEventoProdutoCommandValidator()
    {
        RuleFor(comando => comando.Id)
            .NotEmpty().WithMessage(MensagensErro.IdObrigatorio);

        RuleFor(comando => comando.UsuarioId)
            .NotEmpty().WithMessage(MensagensErro.IdObrigatorio);

        RuleFor(comando => comando.Preco)
            .GreaterThan(0).WithMessage(MensagensErro.PrecoInvalido);
    }
}
