using AgapeApp.Domain.Resources;
using FluentValidation;

namespace AgapeApp.Application.EventoProdutos.Commands.AjustarEstoqueEventoProduto;

public class AjustarEstoqueEventoProdutoCommandValidator
    : AbstractValidator<AjustarEstoqueEventoProdutoCommand>
{
    public AjustarEstoqueEventoProdutoCommandValidator()
    {
        RuleFor(comando => comando.Id)
            .NotEmpty().WithMessage(MensagensErro.IdObrigatorio);

        RuleFor(comando => comando.UsuarioId)
            .NotEmpty().WithMessage(MensagensErro.IdObrigatorio);

        RuleFor(comando => comando.Quantidade)
            .NotEqual(0).WithMessage(MensagensErro.AjusteEstoqueInvalido);

        RuleFor(comando => comando.Observacao)
            .MaximumLength(300).WithMessage(MensagensErro.ObservacaoTamanhoMaximo);
    }
}
