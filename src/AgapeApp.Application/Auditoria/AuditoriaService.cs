using System.Text.Json;
using AgapeApp.Application.Interfaces;
using AgapeApp.Domain.Entities;

namespace AgapeApp.Application.Auditoria;

/// <summary>
/// Registra ações auditáveis na unidade de trabalho atual (persistidas junto do restante, na mesma transação).
/// </summary>
public class AuditoriaService
{
    private readonly ILogAuditoriaRepository _logAuditoriaRepository;

    public AuditoriaService(ILogAuditoriaRepository logAuditoriaRepository)
    {
        _logAuditoriaRepository = logAuditoriaRepository;
    }

    public void Registrar(
        Guid usuarioId,
        string acao,
        string entidade,
        Guid entidadeId,
        Guid? eventoId = null,
        object? dados = null,
        string? dispositivo = null)
    {
        var dadosJson = dados is null ? null : JsonSerializer.Serialize(dados);

        _logAuditoriaRepository.Adicionar(
            LogAuditoria.Criar(usuarioId, acao, entidade, entidadeId, eventoId, dadosJson, dispositivo));
    }
}
