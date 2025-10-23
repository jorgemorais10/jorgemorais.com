using hub.Models;

namespace Hub.Services;

public record Conns(
    string? Empresa,
    string? CS_INFGEST,
    string? CS_INFGEST_SQL,
    string? CS_NAV,
    string? CS_BC,
    string? CS_SHAREPOINT,
    string? Share_FichaProduto,
    string? Share_SalesDocs,
    string? WS_NAV,
    string? WS_NAV_SOAP,
    string? CS_XD,
    string? CS_SMARTTICKET,
    string? CS_BC_Master
);

public interface IConfigAplicacaoProvider
{
    Task<ConfigAplicacao> GetAsync(string? empresa = null, bool bdTeste = false, CancellationToken ct = default);
}
