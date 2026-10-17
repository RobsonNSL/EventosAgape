using AgapeApp.Domain.Common;
using AgapeApp.Domain.Enums;
using AgapeApp.Domain.Exceptions;
using AgapeApp.Domain.Resources;

namespace AgapeApp.Domain.Entities;

/// <summary>
/// Evento da igreja (ex.: pastelaria, bazar). Vários eventos podem rodar ao mesmo tempo.
/// O saldo dos cartões e o estoque são sempre por evento.
/// </summary>
public class Evento : BaseEntity
{
    // Construtor privado exigido pelo Entity Framework.
    private Evento()
    {
    }

    public string Nome { get; private set; } = null!;

    public DateOnly DataInicio { get; private set; }

    /// <summary>Data de fim (opcional; para eventos de um único dia).</summary>
    public DateOnly? DataFim { get; private set; }

    public SituacaoEvento Situacao { get; private set; }

    public Guid CriadoPor { get; private set; }

    /// <summary>
    /// Cria um novo evento em rascunho.
    /// </summary>
    public static Evento Criar(string nome, DateOnly dataInicio, DateOnly? dataFim, Guid criadoPor)
    {
        ValidarNome(nome);
        ValidarDatas(dataInicio, dataFim);

        return new Evento
        {
            Nome = nome.Trim(),
            DataInicio = dataInicio,
            DataFim = dataFim,
            Situacao = SituacaoEvento.Rascunho,
            CriadoPor = criadoPor
        };
    }

    /// <summary>
    /// Atualiza nome e datas. Não é permitido em evento encerrado.
    /// </summary>
    public void Atualizar(string nome, DateOnly dataInicio, DateOnly? dataFim)
    {
        GarantirNaoEncerrado();
        ValidarNome(nome);
        ValidarDatas(dataInicio, dataFim);

        Nome = nome.Trim();
        DataInicio = dataInicio;
        DataFim = dataFim;
        MarcarComoAtualizada();
    }

    /// <summary>
    /// Abre o evento para operações (somente a partir de Rascunho).
    /// </summary>
    public void Abrir()
    {
        if (Situacao != SituacaoEvento.Rascunho)
            throw new RegraNegocioException(MensagensErro.EventoApenasRascunhoPodeAbrir);

        Situacao = SituacaoEvento.Aberto;
        MarcarComoAtualizada();
    }

    /// <summary>
    /// Encerra o evento (somente a partir de Aberto).
    /// </summary>
    public void Encerrar()
    {
        if (Situacao != SituacaoEvento.Aberto)
            throw new RegraNegocioException(MensagensErro.EventoApenasAbertoPodeEncerrar);

        Situacao = SituacaoEvento.Encerrado;
        MarcarComoAtualizada();
    }

    public bool EstaAberto() => Situacao == SituacaoEvento.Aberto;

    public bool EstaEncerrado() => Situacao == SituacaoEvento.Encerrado;

    /// <summary>
    /// Lança regra de negócio se o evento já estiver encerrado (bloqueia alterações de cadastro).
    /// </summary>
    public void GarantirNaoEncerrado()
    {
        if (EstaEncerrado())
            throw new RegraNegocioException(MensagensErro.EventoNaoPodeSerAlterado);
    }

    private static void ValidarNome(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new RegraNegocioException(MensagensErro.NomeObrigatorio);
    }

    private static void ValidarDatas(DateOnly dataInicio, DateOnly? dataFim)
    {
        if (dataFim.HasValue && dataFim.Value < dataInicio)
            throw new RegraNegocioException(MensagensErro.DataFimAnteriorInicio);
    }
}
