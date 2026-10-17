namespace AgapeApp.Domain.Enums;

/// <summary>
/// Perfis de acesso do sistema.
/// </summary>
public enum PerfilUsuario
{
    /// <summary>Administrador: gerencia usuários, eventos, produtos, estoque e relatórios.</summary>
    Administrador = 1,

    /// <summary>Operador: realiza recargas e vendas no setor designado.</summary>
    Operador = 2
}
