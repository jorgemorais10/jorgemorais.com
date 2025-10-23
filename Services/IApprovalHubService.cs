using hub.Models;

namespace Hub.Services;

public interface IApprovalHubService
{
    // ApprovalHub operations
    Task<int> CreateApprovalHubAsync(ApprovalHub approval, CancellationToken ct = default);
    Task<ApprovalHub?> GetApprovalHubByIdAsync(int id, CancellationToken ct = default);
    Task<List<ApprovalHub>> GetAllApprovalHubsAsync(CancellationToken ct = default);
    Task<List<ApprovalHub>> SearchApprovalHubsAsync(
        string? fornecedor = null,
        string? estado = null,
        DateTime? dataInicio = null,
        DateTime? dataFim = null,
        CancellationToken ct = default);
    Task<bool> UpdateApprovalHubAsync(ApprovalHub approval, CancellationToken ct = default);
    Task<bool> DeleteApprovalHubAsync(int id, CancellationToken ct = default);
    Task<bool> UpdateEstadoAsync(int id, string estado, CancellationToken ct = default);

    // Dimensões operations
    Task<int> CreateDimensaoAsync(IdsDimensao dimensao, CancellationToken ct = default);
    Task<List<IdsDimensao>> GetDimensoesByApprovalHubIdAsync(int approvalHubId, CancellationToken ct = default);
    Task<bool> DeleteDimensaoAsync(int id, CancellationToken ct = default);
    Task<bool> DeleteDimensoesByApprovalHubIdAsync(int approvalHubId, CancellationToken ct = default);

    // Anexos operations
    Task<int> CreateAnexoAsync(Anexo anexo, CancellationToken ct = default);
    Task<Anexo?> GetAnexoByIdAsync(int id, CancellationToken ct = default);
    Task<List<Anexo>> GetAnexosByApprovalHubIdAsync(int approvalHubId, CancellationToken ct = default);
    Task<bool> DeleteAnexoAsync(int id, CancellationToken ct = default);

    // Utility
    Task<List<string>> GetDistinctFornecedoresAsync(CancellationToken ct = default);
    Task<List<string>> GetDistinctEstadosAsync(CancellationToken ct = default);
}
