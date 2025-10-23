using System.Data;
using hub.Helpers;
using hub.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Caching.Memory;

namespace Hub.Services;

public sealed class ConfigAplicacaoProvider : IConfigAplicacaoProvider
{
    private readonly IConfiguration _cfg;
    private readonly IHttpContextAccessor _http;
    private readonly IMemoryCache _cache;
    private readonly ILogger<ConfigAplicacaoProvider> _log;

    public ConfigAplicacaoProvider(
        IConfiguration cfg,
        IHttpContextAccessor http,
        IMemoryCache cache,
        ILogger<ConfigAplicacaoProvider> log)
    {
        _cfg = cfg;
        _http = http;
        _cache = cache;
        _log = log;
    }

    public async Task<ConfigAplicacao> GetAsync(string? empresa = null, bool bdTeste = false, CancellationToken ct = default)
    {
        var session = _http.HttpContext?.Session;

        // 1) Tentar sessão
        empresa ??= session?.GetString("empresa");
        string? aplicacaoId = session?.GetString("AplicacaoID");

        // 1.1) Fallback: descobrir empresa/aplicação do utilizador
        if (string.IsNullOrWhiteSpace(empresa) || string.IsNullOrWhiteSpace(aplicacaoId) || aplicacaoId == "0")
        {
            var rawName = _http.HttpContext?.User?.Identity?.Name ?? "";
            var username = rawName.Contains('\\') ? rawName.Split('\\').Last() : rawName;
            username = (username ?? "").Trim().ToUpperInvariant();

            try
            {
                var conStrInfgest = ConfigurationHelper.GetSetting("ConnectionStrings:InfgestMVC");
                var empresas = Empresa.GetEmpresas(username, conStrInfgest ?? "");
                var def = empresas.FirstOrDefault(e => e.EmpresaDefeito_ID == e.Empresa_ID)
                         ?? empresas.FirstOrDefault();

                if (def != null)
                {
                    empresa ??= def.Nome;

                    // Se tiver valor (>0), usa-o
                    if (def?.Aplicacao_ID is int appId && appId > 0)
                    {
                        aplicacaoId = appId.ToString();
                    }

                    session?.SetString("empresa", empresa);
                    if (!string.IsNullOrWhiteSpace(aplicacaoId))
                        session?.SetString("AplicacaoID", aplicacaoId);
                    session?.SetString("BD", "Infgest");
                }
            }
            catch (Exception ex)
            {
                _log.LogWarning(ex, "Falha ao obter empresas do utilizador '{User}'.", username);
            }
        }

        // 2) Connection string da BD de configuração (prod/testes)
        var csName = bdTeste ? "InfgestMVC_TESTES" : "InfgestMVC";
        var conexao = _cfg.GetConnectionString(csName)
                     ?? throw new InvalidOperationException($"ConnectionStrings:{csName} não configurada.");

        // 3) Cache por empresa+ambiente
        var cacheKey = $"config:{empresa ?? "_anon_"}:{csName}";
        if (_cache.TryGetValue(cacheKey, out ConfigAplicacao? cacheHit) && cacheHit != null)
            return cacheHit;

        using var cn = new SqlConnection(conexao);
        await cn.OpenAsync(ct);

        // 4) Descobrir Aplicacao_ID pela empresa caso ainda falte ou seja "0"
        if ((string.IsNullOrWhiteSpace(aplicacaoId) || aplicacaoId == "0") && !string.IsNullOrWhiteSpace(empresa))
        {
            const string sqlApp = @"
                SELECT TOP (1) a.Aplicacao_ID
                FROM CONFIG_APLICACAO a
                JOIN CONFIG_EMPRESA e ON e.Empresa_ID = a.Empresa_ID
                WHERE UPPER(e.Nome) = UPPER(@empresa)
                  AND a.Aplicacao_ID > 19;";

            using var cmdApp = new SqlCommand(sqlApp, cn);
            cmdApp.Parameters.Add(new SqlParameter("@empresa", SqlDbType.NVarChar, 200) { Value = empresa! });

            var result = await cmdApp.ExecuteScalarAsync(ct);
            aplicacaoId = result?.ToString();

            if (!string.IsNullOrWhiteSpace(aplicacaoId) && aplicacaoId != "0")
                session?.SetString("AplicacaoID", aplicacaoId);
        }

        if (string.IsNullOrWhiteSpace(aplicacaoId) || aplicacaoId == "0")
            throw new InvalidOperationException("AplicacaoID não encontrada (nem na sessão nem por empresa).");

        // 5) Ler CONFIG_APLICACAO
        const string sqlConfig = @"SELECT * FROM CONFIG_APLICACAO WHERE Aplicacao_ID = @AplicacaoID";
        using var cmd = new SqlCommand(sqlConfig, cn);
        cmd.Parameters.Add(new SqlParameter("@AplicacaoID", SqlDbType.Int) { Value = int.Parse(aplicacaoId) });

        using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
            throw new InvalidOperationException($"CONFIG_APLICACAO não encontrada para Aplicacao_ID={aplicacaoId}");

        var item = new ConfigAplicacao
        {
            AplicacaoID = reader["Aplicacao_ID"]?.ToString(),
            Nome = reader["Nome"]?.ToString(),
            CS_INFGEST = reader["CS_INFGEST"]?.ToString(),
            CS_INFGEST_SQL = reader["CS_INFGEST_SQL"]?.ToString(),
            CS_NAV = reader["CS_NAV"]?.ToString(),
            CS_SHAREPOINT = reader["CS_SHAREPOINT"]?.ToString(),
            share_FichaProduto = reader["share_FichaProduto"]?.ToString(),
            Share_SalesDocs = reader["Share_SalesDocs"]?.ToString(),
            WS_NAV = reader["WS_NAV"]?.ToString(),
            WS_NAV_SOAP = reader["WS_NAV_SOAP"]?.ToString(),
            CS_XD = reader["CS_XD"]?.ToString(),
            CS_SMARTTICKET = reader["CS_SMARTTICKET"]?.ToString(),
            CS_BC_Master = reader["CS_BC_Master"]?.ToString(),
            CS_BC = reader["CS_BC"]?.ToString(),
            Empresa = empresa
        };

        // 6) Nome da BD a partir do CS_INFGEST_SQL
        try
        {
            if (!string.IsNullOrWhiteSpace(item.CS_INFGEST_SQL))
            {
                var b = new SqlConnectionStringBuilder(item.CS_INFGEST_SQL);
                item.NomeBD_INFGEST_SQL = b.InitialCatalog;
            }
        }
        catch
        {
            // Mantém comportamento antigo se a string não estiver no formato padrão
            var cs = item.CS_INFGEST_SQL?.ToLowerInvariant();
            if (!string.IsNullOrEmpty(cs) && cs.Contains("database"))
            {
                var start = cs.IndexOf("database", StringComparison.Ordinal);
                var eq = cs.IndexOf('=', start);
                var end = cs.IndexOf(';', eq + 1);
                if (eq > 0 && end > eq)
                    item.NomeBD_INFGEST_SQL = item.CS_INFGEST_SQL!.Substring(eq + 1, end - (eq + 1)).Trim();
            }
        }

        // 7) Coluna opcional Hub_SQL
        try
        {
            var ord = reader.GetOrdinal("Hub_SQL");
            if (ord >= 0 && !reader.IsDBNull(ord))
                item.Hub_SQL = reader["Hub_SQL"]?.ToString();
        }
        catch { /* coluna pode não existir */ }

        // 8) Actualizar sessão útil para outras partes
        session?.SetString("BD", item.NomeBD_INFGEST_SQL ?? string.Empty);

        // 9) Cache
        _cache.Set(cacheKey, item, TimeSpan.FromMinutes(5));

        return item;
    }
}
