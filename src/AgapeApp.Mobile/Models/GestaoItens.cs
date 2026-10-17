using AgapeApp.Mobile.Helpers;

namespace AgapeApp.Mobile.Models;

/// <summary>Cor e texto da situação do evento (preparação, aberto ou encerrado).</summary>
public static class SituacaoEvento
{
    public static string Texto(string situacao) => situacao switch
    {
        "Aberto" => "Aberto",
        "Encerrado" => "Encerrado",
        _ => "Em preparação"
    };

    public static Color Cor(string situacao) => situacao switch
    {
        "Aberto" => Cores.VerdeTexto,
        "Encerrado" => Cores.CinzaTexto,
        _ => Cores.AmbarTexto
    };

    public static Color Fundo(string situacao) => situacao switch
    {
        "Aberto" => Cores.VerdeFundo,
        "Encerrado" => Cores.CinzaFundo,
        _ => Cores.AmbarFundo
    };
}

/// <summary>Usuário na lista de gestão (iniciais e situação prontas para exibição).</summary>
public class UsuarioItem
{
    public required Guid Id { get; init; }
    public required string Nome { get; init; }
    public required string Email { get; init; }
    public required string Perfil { get; init; }
    public required bool Ativo { get; init; }

    public string Iniciais => Formatos.Iniciais(Nome);

    public bool EhAdministrador => Perfil == "Administrador";

    /// <summary>Perfil do operador é o complemento; usado nos chips da lista.</summary>
    public bool EhOperador => !EhAdministrador;

    public bool Inativo => !Ativo;

    public string TextoAtivacao => Ativo ? "Desativar acesso" : "Ativar acesso";

    public static UsuarioItem De(UsuarioDTO dto) => new()
    {
        Id = dto.Id,
        Nome = dto.Nome,
        Email = dto.Email,
        Perfil = dto.Perfil,
        Ativo = dto.Ativo
    };
}

/// <summary>Produto do catálogo na lista de gestão.</summary>
public class ProdutoItem
{
    public required Guid Id { get; init; }
    public required string Nome { get; init; }
    public required decimal? PrecoPadrao { get; init; }
    public required bool Ativo { get; init; }

    public string TextoPreco => PrecoPadrao is decimal preco
        ? $"Preço padrão {Formatos.Moeda(preco)}"
        : "Sem preço padrão";

    public string TextoSituacao => Ativo ? "Ativo" : "Inativo";

    public Color SituacaoFundo => Ativo ? Cores.VerdeFundo : Cores.CinzaFundo;

    public Color SituacaoCor => Ativo ? Cores.VerdeTexto : Cores.CinzaTexto;

    public string TextoAtivacao => Ativo ? "Desativar produto" : "Ativar produto";

    public static ProdutoItem De(ProdutoDTO dto) => new()
    {
        Id = dto.Id,
        Nome = dto.Nome,
        PrecoPadrao = dto.PrecoPadrao,
        Ativo = dto.Ativo
    };
}

/// <summary>Evento na lista de gestão (todos os eventos, sem setor de operador).</summary>
public class EventoAdminItem
{
    public required Guid Id { get; init; }
    public required string Nome { get; init; }
    public required string Situacao { get; init; }
    public required string DataTexto { get; init; }
    public required DateOnly DataInicio { get; init; }
    public required DateOnly? DataFim { get; init; }

    public string SituacaoTexto => SituacaoEvento.Texto(Situacao);

    public Color SituacaoCor => SituacaoEvento.Cor(Situacao);

    public Color SituacaoFundo => SituacaoEvento.Fundo(Situacao);

    public bool PodeAbrirOuEncerrar => Situacao is "Rascunho" or "Aberto";

    public string TextoAbrirEncerrar => Situacao == "Aberto" ? "Encerrar evento" : "Abrir para vendas";

    public static EventoAdminItem De(EventoDTO dto) => new()
    {
        Id = dto.Id,
        Nome = dto.Nome,
        Situacao = dto.Situacao,
        DataTexto = Formatos.DataEvento(dto.DataInicio, dto.DataFim),
        DataInicio = dto.DataInicio,
        DataFim = dto.DataFim
    };
}

/// <summary>Cartão na lista de gestão.</summary>
public class CartaoItem
{
    public required Guid Id { get; init; }
    public required string Codigo { get; init; }
    public required bool Ativo { get; init; }
    public required string CriadoEmTexto { get; init; }

    public string TextoSituacao => Ativo ? "Ativo" : "Inativo";

    public Color SituacaoFundo => Ativo ? Cores.VerdeFundo : Cores.CinzaFundo;

    public Color SituacaoCor => Ativo ? Cores.VerdeTexto : Cores.CinzaTexto;

    public string TextoAtivacao => Ativo ? "Desativar cartão" : "Ativar cartão";

    public static CartaoItem De(CartaoDTO dto) => new()
    {
        Id = dto.Id,
        Codigo = dto.Codigo,
        Ativo = dto.Ativo,
        CriadoEmTexto = $"Cadastrado em {dto.CriadoEm.ToLocalTime():dd/MM/yyyy}"
    };
}

/// <summary>Setor do evento na tela de detalhes.</summary>
public class SetorItem
{
    public required Guid Id { get; init; }
    public required string Nome { get; init; }

    public static SetorItem De(SetorDTO dto) => new()
    {
        Id = dto.Id,
        Nome = dto.Nome
    };
}

/// <summary>Produto vinculado ao evento na tela de detalhes.</summary>
public class EventoProdutoItem
{
    public required Guid Id { get; init; }
    public required Guid ProdutoId { get; init; }
    public required string ProdutoNome { get; init; }
    public required decimal Preco { get; init; }
    public required bool ControlaEstoque { get; init; }
    public required int QuantidadeEstoque { get; init; }

    public string TextoPreco => Formatos.Moeda(Preco);

    public string TextoEstoque => !ControlaEstoque
        ? "Não controla estoque"
        : QuantidadeEstoque == 0
            ? "Estoque zerado"
            : QuantidadeEstoque == 1 ? "1 unidade em estoque" : $"{QuantidadeEstoque} unidades em estoque";

    public bool EstoqueZerado => ControlaEstoque && QuantidadeEstoque <= 0;

    public Color EstoqueFundo => EstoqueZerado ? Cores.PerigoFundo : Cores.CinzaFundo;

    public Color EstoqueCor => EstoqueZerado ? Cores.PerigoTexto : Cores.CinzaTexto;

    public static EventoProdutoItem De(EventoProdutoDTO dto) => new()
    {
        Id = dto.Id,
        ProdutoId = dto.ProdutoId,
        ProdutoNome = dto.ProdutoNome,
        Preco = dto.Preco,
        ControlaEstoque = dto.ControlaEstoque,
        QuantidadeEstoque = dto.QuantidadeEstoque
    };
}

/// <summary>Operador designado a um setor do evento.</summary>
public class OperadorItem
{
    public required Guid Id { get; init; }
    public required Guid UsuarioId { get; init; }
    public required string UsuarioNome { get; init; }
    public required Guid SetorId { get; init; }
    public required string SetorNome { get; init; }
    public required bool Habilitado { get; init; }

    public string Iniciais => Formatos.Iniciais(UsuarioNome);

    public string TextoSituacao => Habilitado ? "Habilitado" : "Bloqueado";

    public Color SituacaoFundo => Habilitado ? Cores.VerdeFundo : Cores.CinzaFundo;

    public Color SituacaoCor => Habilitado ? Cores.VerdeTexto : Cores.CinzaTexto;

    public string TextoAlternar => Habilitado ? "Bloquear no setor" : "Habilitar no setor";

    public static OperadorItem De(EventoOperadorDTO dto) => new()
    {
        Id = dto.Id,
        UsuarioId = dto.UsuarioId,
        UsuarioNome = dto.UsuarioNome,
        SetorId = dto.SetorId,
        SetorNome = dto.SetorNome,
        Habilitado = dto.Habilitado
    };
}
