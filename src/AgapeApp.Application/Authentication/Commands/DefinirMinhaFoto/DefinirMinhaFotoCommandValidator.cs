using AgapeApp.Application.Common;
using AgapeApp.Domain.Resources;
using FluentValidation;

namespace AgapeApp.Application.Authentication.Commands.DefinirMinhaFoto;

public class DefinirMinhaFotoCommandValidator : AbstractValidator<DefinirMinhaFotoCommand>
{
    public DefinirMinhaFotoCommandValidator()
    {
        RuleFor(comando => comando.UsuarioId)
            .NotEmpty().WithMessage(MensagensErro.IdObrigatorio);

        RuleFor(comando => comando.Conteudo)
            .NotNull().WithMessage(MensagensErro.FotoObrigatoria);

        RuleFor(comando => comando.NomeArquivo)
            .Must(FotoPerfil.ExtensaoPermitida).WithMessage(MensagensErro.FotoFormatoInvalido);

        RuleFor(comando => comando.Tamanho)
            .GreaterThan(0).WithMessage(MensagensErro.FotoArquivoVazio)
            .LessThanOrEqualTo(FotoPerfil.TamanhoMaximoBytes).WithMessage(MensagensErro.FotoMuitoGrande);
    }
}
