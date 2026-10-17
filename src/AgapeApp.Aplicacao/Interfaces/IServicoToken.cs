using AgapeApp.Dominio.Entidades;

namespace AgapeApp.Aplicacao.Interfaces;

/// <summary>
/// Geração de tokens de acesso (JWT) e de renovação (refresh token).
/// </summary>
public interface IServicoToken
{
    /// <summary>Gera o token de acesso (JWT) do usuário, junto com a data de expiração.</summary>
    (string Token, DateTimeOffset ExpiraEm) GerarTokenAcesso(Usuario usuario);

    /// <summary>Gera um novo valor bruto de token de renovação (aleatório e seguro).</summary>
    string GerarTokenRenovacao();

    /// <summary>Calcula o hash do token de renovação para armazenamento no banco.</summary>
    string CalcularHashTokenRenovacao(string token);

    /// <summary>Retorna a data de expiração para um novo token de renovação.</summary>
    DateTimeOffset ObterExpiracaoTokenRenovacao();
}
