using System.Globalization;
using AgapeApp.Mobile.Helpers;
using AgapeApp.Mobile.Models;
using SQLite;

namespace AgapeApp.Mobile.Services;

/// <summary>
/// Cache local (SQLite) de leitura: eventos do operador, produtos do setor e as
/// últimas operações feitas neste aparelho. A escrita na API continua exigindo
/// internet — o cache só evita a tela vazia quando a rede cai.
/// Falhas de cache são silenciosas: nunca impedem a operação principal.
/// </summary>
public class CacheService
{
    private readonly SemaphoreSlim _trava = new(1, 1);
    private SQLiteAsyncConnection? _banco;

    // ===== Eventos do operador =====

    public Task SalvarEventosAsync(IReadOnlyList<MeuEventoDTO> eventos) => ComBancoAsync(async banco =>
    {
        await banco.DeleteAllAsync<EventoCacheRow>();
        await banco.InsertAllAsync(eventos.Select(e => new EventoCacheRow
        {
            EventoId = e.EventoId,
            SetorId = e.SetorId,
            Nome = e.Nome,
            DataInicio = e.DataInicio.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            DataFim = e.DataFim?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            Situacao = e.Situacao,
            SetorNome = e.SetorNome,
            Habilitado = e.Habilitado,
            AtualizadoEmTicks = DateTime.UtcNow.Ticks
        }));
    });

    public Task<IReadOnlyList<MeuEventoDTO>> LerEventosAsync() => ComBancoAsync<IReadOnlyList<MeuEventoDTO>>(async banco =>
    {
        var linhas = await banco.Table<EventoCacheRow>()
            .OrderBy(e => e.Nome)
            .ToListAsync();

        return linhas.Select(l => new MeuEventoDTO(
            l.EventoId,
            l.Nome,
            ParaData(l.DataInicio),
            string.IsNullOrWhiteSpace(l.DataFim) ? null : ParaData(l.DataFim),
            l.Situacao,
            l.SetorId,
            l.SetorNome,
            l.Habilitado)).ToList();
    }, []);

    // ===== Produtos do setor =====

    public Task SalvarProdutosDoSetorAsync(Guid setorId, IReadOnlyList<EventoProdutoDTO> produtos) => ComBancoAsync(async banco =>
    {
        var antigos = await banco.Table<ProdutoCacheRow>()
            .Where(p => p.SetorId == setorId)
            .ToListAsync();

        foreach (var antigo in antigos)
            await banco.DeleteAsync(antigo);

        await banco.InsertAllAsync(produtos.Select(p => new ProdutoCacheRow
        {
            SetorId = setorId,
            EventoProdutoId = p.Id,
            EventoId = p.EventoId,
            ProdutoId = p.ProdutoId,
            ProdutoNome = p.ProdutoNome,
            Preco = (double)p.Preco,
            ControlaEstoque = p.ControlaEstoque,
            QuantidadeEstoque = p.QuantidadeEstoque,
            CaminhoImagem = p.ProdutoCaminhoImagem,
            AtualizadoEmTicks = DateTime.UtcNow.Ticks
        }));
    });

    public Task<IReadOnlyList<EventoProdutoDTO>> LerProdutosDoSetorAsync(Guid setorId) =>
        ComBancoAsync<IReadOnlyList<EventoProdutoDTO>>(async banco =>
        {
            var linhas = await banco.Table<ProdutoCacheRow>()
                .Where(p => p.SetorId == setorId)
                .OrderBy(p => p.ProdutoNome)
                .ToListAsync();

            return linhas.Select(l => new EventoProdutoDTO(
                l.EventoProdutoId,
                l.EventoId,
                l.ProdutoId,
                l.ProdutoNome,
                (decimal)l.Preco,
                l.ControlaEstoque,
                l.QuantidadeEstoque,
                l.CaminhoImagem)).ToList();
        }, []);

    // ===== Últimas operações deste aparelho =====

    public Task RegistrarRecargaAsync(RecargaResponse resposta, string setorNome) => ComBancoAsync(async banco =>
    {
        await banco.InsertAsync(new OperacaoRecenteRow
        {
            EventoId = resposta.EventoId,
            Tipo = "Recarga",
            CartaoCodigo = resposta.CartaoCodigo,
            Valor = (double)resposta.Valor,
            SetorNome = setorNome,
            CriadoEmTicks = resposta.CriadoEm.UtcTicks
        });

        await LimitarRecentesAsync(banco, resposta.EventoId);
    });

    public Task RegistrarVendaAsync(VendaResponse resposta, string setorNome) => ComBancoAsync(async banco =>
    {
        await banco.InsertAsync(new OperacaoRecenteRow
        {
            EventoId = resposta.EventoId,
            Tipo = "Venda",
            CartaoCodigo = resposta.CartaoCodigo,
            Valor = (double)resposta.Total,
            SetorNome = setorNome,
            Produtos = string.Join(", ", resposta.Itens.Select(i => $"{i.Quantidade}× {i.ProdutoNome}")),
            CriadoEmTicks = resposta.CriadoEm.UtcTicks
        });

        await LimitarRecentesAsync(banco, resposta.EventoId);
    });

    public Task<IReadOnlyList<OperacaoRecente>> LerRecentesAsync(Guid eventoId, int limite = 4) =>
        ComBancoAsync<IReadOnlyList<OperacaoRecente>>(async banco =>
        {
            var linhas = await banco.Table<OperacaoRecenteRow>()
                .Where(o => o.EventoId == eventoId)
                .OrderByDescending(o => o.Id)
                .Take(limite)
                .ToListAsync();

            return linhas.Select(DeLinha).ToList();
        }, []);

    private static OperacaoRecente DeLinha(OperacaoRecenteRow linha)
    {
        var recarga = linha.Tipo == "Recarga";
        var criadoEm = new DateTimeOffset(linha.CriadoEmTicks, TimeSpan.Zero).ToLocalTime();

        return new OperacaoRecente
        {
            Tipo = linha.Tipo,
            CartaoCodigo = linha.CartaoCodigo,
            Detalhe = recarga
                ? linha.SetorNome ?? string.Empty
                : linha.Produtos is { Length: > 0 } produtos ? produtos : linha.SetorNome ?? string.Empty,
            ValorTexto = Formatos.Moeda((decimal)linha.Valor),
            HoraTexto = criadoEm.ToString("HH:mm", Formatos.Cultura),
            Icone = recarga ? "\uf0d6" : "\uf07a",
            Cor = recarga ? Cores.AmbarTexto : Cores.VerdeTexto,
            Fundo = recarga ? Cores.AmbarFundo : Cores.VerdeFundo
        };
    }

    private static async Task LimitarRecentesAsync(SQLiteAsyncConnection banco, Guid eventoId)
    {
        var excedentes = await banco.Table<OperacaoRecenteRow>()
            .Where(o => o.EventoId == eventoId)
            .OrderByDescending(o => o.Id)
            .Skip(50)
            .ToListAsync();

        foreach (var linha in excedentes)
            await banco.DeleteAsync(linha);
    }

    // ===== Infraestrutura =====

    private async Task<SQLiteAsyncConnection?> ObterBancoAsync()
    {
        if (_banco is not null)
            return _banco;

        await _trava.WaitAsync();
        try
        {
            if (_banco is null)
            {
                var caminho = Path.Combine(FileSystem.AppDataDirectory, "agape-cache.db3");
                var banco = new SQLiteAsyncConnection(caminho);

                await banco.CreateTableAsync<EventoCacheRow>();
                await banco.CreateTableAsync<ProdutoCacheRow>();
                await banco.CreateTableAsync<OperacaoRecenteRow>();

                _banco = banco;
            }
        }
        finally
        {
            _trava.Release();
        }

        return _banco;
    }

    private async Task ComBancoAsync(Func<SQLiteAsyncConnection, Task> acao)
    {
        try
        {
            var banco = await ObterBancoAsync();
            if (banco is not null)
                await acao(banco);
        }
        catch
        {
            // Cache é melhor esforço: qualquer falha local é ignorada.
        }
    }

    private async Task<T> ComBancoAsync<T>(Func<SQLiteAsyncConnection, Task<T>> acao, T padrao)
    {
        try
        {
            var banco = await ObterBancoAsync();
            return banco is null ? padrao : await acao(banco);
        }
        catch
        {
            return padrao;
        }
    }

    private static DateOnly ParaData(string valor)
        => DateOnly.ParseExact(valor, "yyyy-MM-dd", CultureInfo.InvariantCulture);

    // ===== Tabelas =====

    [Table("eventos_cache")]
    public class EventoCacheRow
    {
        [PrimaryKey]
        public Guid EventoId { get; set; }

        [PrimaryKey]
        public Guid SetorId { get; set; }

        public string Nome { get; set; } = string.Empty;
        public string DataInicio { get; set; } = string.Empty;
        public string? DataFim { get; set; }
        public string Situacao { get; set; } = string.Empty;
        public string SetorNome { get; set; } = string.Empty;
        public bool Habilitado { get; set; }
        public long AtualizadoEmTicks { get; set; }
    }

    [Table("produtos_cache")]
    public class ProdutoCacheRow
    {
        [PrimaryKey]
        public Guid SetorId { get; set; }

        [PrimaryKey]
        public Guid EventoProdutoId { get; set; }

        public Guid EventoId { get; set; }
        public Guid ProdutoId { get; set; }
        public string ProdutoNome { get; set; } = string.Empty;
        public double Preco { get; set; }
        public bool ControlaEstoque { get; set; }
        public int QuantidadeEstoque { get; set; }
        public string? CaminhoImagem { get; set; }
        public long AtualizadoEmTicks { get; set; }
    }

    [Table("operacoes_recentes")]
    public class OperacaoRecenteRow
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public Guid EventoId { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public string CartaoCodigo { get; set; } = string.Empty;
        public double Valor { get; set; }
        public string? SetorNome { get; set; }
        public string? Produtos { get; set; }
        public long CriadoEmTicks { get; set; }
    }
}
