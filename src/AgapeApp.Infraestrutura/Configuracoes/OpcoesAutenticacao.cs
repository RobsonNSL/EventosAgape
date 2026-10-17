namespace AgapeApp.Infraestrutura.Configuracoes;

/// <summary>
/// Configurações de autenticação (seção "Autenticacao" do appsettings).
/// </summary>
public class OpcoesAutenticacao
{
    public const string SecaoConfiguracao = "Autenticacao";

    /// <summary>Chave secreta usada para assinar os tokens JWT (mínimo 32 caracteres).</summary>
    public string ChaveSecreta { get; set; } = string.Empty;

    /// <summary>Emissor do token (iss).</summary>
    public string Emissor { get; set; } = "AgapeApp";

    /// <summary>Público-alvo do token (aud).</summary>
    public string Audiencia { get; set; } = "AgapeApp";

    /// <summary>Validade do token de acesso, em minutos.</summary>
    public int MinutosTokenAcesso { get; set; } = 30;

    /// <summary>Validade do token de renovação, em dias.</summary>
    public int DiasTokenRenovacao { get; set; } = 7;
}
