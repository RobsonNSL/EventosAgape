namespace AgapeApp.Dominio.Comum;

/// <summary>
/// Classe base para todas as entidades do domínio.
/// Identificador em UUID (versão 7, ordenável) e datas em UTC.
/// </summary>
public abstract class EntidadeBase
{
    protected EntidadeBase()
    {
        Id = Guid.CreateVersion7();
        CriadoEm = DateTimeOffset.UtcNow;
        AtualizadoEm = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; protected set; }

    public DateTimeOffset CriadoEm { get; protected set; }

    public DateTimeOffset AtualizadoEm { get; protected set; }

    /// <summary>
    /// Atualiza a data da última modificação da entidade.
    /// </summary>
    public void MarcarComoAtualizada() => AtualizadoEm = DateTimeOffset.UtcNow;
}
