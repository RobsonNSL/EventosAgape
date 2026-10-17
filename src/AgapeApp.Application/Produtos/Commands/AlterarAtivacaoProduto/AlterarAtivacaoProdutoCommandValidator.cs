using AgapeApp.Domain.Resources;
using FluentValidation;

namespace AgapeApp.Application.Produtos.Commands.AlterarAtivacaoProduto;

public class AlterarAtivacaoProdutoCommandValidator : AbstractValidator<AlterarAtivacaoProdutoCommand>
{
    public AlterarAtivacaoProdutoCommandValidator()
    {
        RuleFor(comando => comando.Id)
            .NotEmpty().WithMessage(MensagensErro.IdObrigatorio);
    }
}
