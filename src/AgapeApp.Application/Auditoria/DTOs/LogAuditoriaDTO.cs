namespace AgapeApp.Application.Auditoria.DTOs;

/// <summary>
/// Registro de auditoria para o relatório do dashboard do admin.
/// </summary>
public record LogAuditoriaDTO(
    Guid Id,
    string UsuarioNome,
    string Acao,
    string Entidade,
    Guid EntidadeId,
    string? Dados,
    string? Dispositivo,
    DateTimeOffset CriadoEm);
