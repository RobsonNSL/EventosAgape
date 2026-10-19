using AgapeApp.Application.Produtos.Commands.AlterarAtivacaoProduto;
using AgapeApp.Application.Produtos.Commands.AtualizarProduto;
using AgapeApp.Application.Produtos.Commands.CriarProduto;
using AgapeApp.Application.Produtos.Commands.DefinirImagemProduto;
using AgapeApp.Application.Produtos.Commands.RemoverImagemProduto;
using AgapeApp.Application.Produtos.DTOs;
using AgapeApp.Application.Produtos.Queries.ListarProdutos;
using AgapeApp.Application.Produtos.Queries.ObterProduto;
using AgapeApp.Domain.Exceptions;
using AgapeApp.Domain.Resources;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgapeApp.Api.Controllers;

/// <summary>
/// Catálogo de produtos. Acesso exclusivo de administradores.
/// </summary>
[ApiController]
[Route("api/produtos")]
[Authorize(Policy = "SomenteAdministrador")]
public class ProdutosController : ControllerBase
{
    private readonly ISender _mediator;

    public ProdutosController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Cadastra um novo produto no catálogo.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ProdutoDTO), StatusCodes.Status201Created)]
    public async Task<ActionResult<ProdutoDTO>> Criar(
        [FromBody] CriarProdutoCommand comando,
        CancellationToken cancellationToken)
    {
        var produto = await _mediator.Send(comando, cancellationToken);
        return Created($"/api/produtos/{produto.Id}", produto);
    }

    /// <summary>Lista os produtos do catálogo (por padrão apenas os ativos).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ProdutoDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProdutoDTO>>> Listar(
        [FromQuery] bool incluirInativos,
        CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new ListarProdutosQuery(incluirInativos), cancellationToken));

    /// <summary>Obtém um produto pelo identificador.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProdutoDTO), StatusCodes.Status200OK)]
    public async Task<ActionResult<ProdutoDTO>> Obter(Guid id, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new ObterProdutoQuery(id), cancellationToken));

    /// <summary>Atualiza nome e preço padrão de um produto (o id vem da rota).</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ProdutoDTO), StatusCodes.Status200OK)]
    public async Task<ActionResult<ProdutoDTO>> Atualizar(
        Guid id,
        [FromBody] AtualizarProdutoCommand comando,
        CancellationToken cancellationToken)
    {
        comando = comando with { Id = id };
        return Ok(await _mediator.Send(comando, cancellationToken));
    }

    /// <summary>Ativa um produto.</summary>
    [HttpPost("{id:guid}/ativar")]
    [ProducesResponseType(typeof(ProdutoDTO), StatusCodes.Status200OK)]
    public async Task<ActionResult<ProdutoDTO>> Ativar(Guid id, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new AlterarAtivacaoProdutoCommand(id, true), cancellationToken));

    /// <summary>Desativa um produto (exclusão lógica).</summary>
    [HttpPost("{id:guid}/desativar")]
    [ProducesResponseType(typeof(ProdutoDTO), StatusCodes.Status200OK)]
    public async Task<ActionResult<ProdutoDTO>> Desativar(Guid id, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new AlterarAtivacaoProdutoCommand(id, false), cancellationToken));

    /// <summary>Envia (ou troca) a imagem do produto: jpg/jpeg/png/webp até 5 MB (multipart, campo "arquivo").</summary>
    [HttpPost("{id:guid}/imagem")]
    [ProducesResponseType(typeof(ProdutoDTO), StatusCodes.Status200OK)]
    public async Task<ActionResult<ProdutoDTO>> DefinirImagem(
        Guid id,
        // Sem [FromForm]: o binding multipart é inferido (com o atributo o Swagger quebra).
        IFormFile? arquivo,
        CancellationToken cancellationToken)
    {
        if (arquivo is null)
            throw new RegraNegocioException(MensagensErro.FotoObrigatoria);

        await using var conteudo = arquivo.OpenReadStream();

        return Ok(await _mediator.Send(
            new DefinirImagemProdutoCommand(id, arquivo.FileName, arquivo.Length, conteudo),
            cancellationToken));
    }

    /// <summary>Remove a imagem do produto.</summary>
    [HttpDelete("{id:guid}/imagem")]
    [ProducesResponseType(typeof(ProdutoDTO), StatusCodes.Status200OK)]
    public async Task<ActionResult<ProdutoDTO>> RemoverImagem(Guid id, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new RemoverImagemProdutoCommand(id), cancellationToken));
}
