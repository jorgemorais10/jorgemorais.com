using System.Data;
using hub.Models;
using Microsoft.Data.SqlClient;

namespace Hub.Services;

public class ApprovalHubService : IApprovalHubService
{
    private readonly IConfigAplicacaoProvider _configProvider;
    private readonly ILogger<ApprovalHubService> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ApprovalHubService(
        IConfigAplicacaoProvider configProvider,
        ILogger<ApprovalHubService> logger,
        IHttpContextAccessor httpContextAccessor)
    {
        _configProvider = configProvider;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    private async Task<SqlConnection> GetConnectionAsync(CancellationToken ct = default)
    {
        var config = await _configProvider.GetAsync(bdTeste: true, ct: ct);
        var connectionString = config.Hub_SQL ?? config.CS_INFGEST_SQL
            ?? throw new InvalidOperationException("Connection string não disponível");

        var conn = new SqlConnection(connectionString);
        await conn.OpenAsync(ct);
        return conn;
    }

    private string GetCurrentUser()
    {
        var rawName = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "SYSTEM";
        return rawName.Contains('\\') ? rawName.Split('\\').Last() : rawName;
    }

    #region ApprovalHub Operations

    public async Task<int> CreateApprovalHubAsync(ApprovalHub approval, CancellationToken ct = default)
    {
        using var conn = await GetConnectionAsync(ct);

        const string sql = @"
            INSERT INTO aprov.approval_hub (
                tipo_fatura, documento_a_criar, comprador, aprovador,
                fornecedor_bc_id, fornecedor_nome, local_livre, observacoes,
                valor_total, num_fatura, data_documento, empresa_nome,
                estado, criado_por, criado_em, atualizado_em
            )
            OUTPUT INSERTED.id_approval_hub
            VALUES (
                @TipoFatura, @DocumentoACriar, @Comprador, @Aprovador,
                @FornecedorBcId, @FornecedorNome, @LocalLivre, @Observacoes,
                @ValorTotal, @NumFatura, @DataDocumento, @EmpresaNome,
                @Estado, @CriadoPor, GETDATE(), GETDATE()
            )";

        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@TipoFatura", approval.TipoFatura ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@DocumentoACriar", approval.DocumentoACriar ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@Comprador", approval.Comprador ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@Aprovador", approval.Aprovador ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@FornecedorBcId", approval.FornecedorBcId ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@FornecedorNome", approval.FornecedorNome ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@LocalLivre", approval.LocalLivre ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@Observacoes", approval.Observacoes ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@ValorTotal", approval.ValorTotal);
        cmd.Parameters.AddWithValue("@NumFatura", approval.NumFatura ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@DataDocumento", approval.DataDocumento ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@EmpresaNome", approval.EmpresaNome ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@Estado", approval.Estado ?? "Pendente");
        cmd.Parameters.AddWithValue("@CriadoPor", approval.CriadoPor ?? GetCurrentUser());

        var result = await cmd.ExecuteScalarAsync(ct);
        return result != null ? Convert.ToInt32(result) : 0;
    }

    public async Task<ApprovalHub?> GetApprovalHubByIdAsync(int id, CancellationToken ct = default)
    {
        using var conn = await GetConnectionAsync(ct);

        const string sql = @"
            SELECT
                id_approval_hub, tipo_fatura, documento_a_criar, comprador, aprovador,
                fornecedor_bc_id, fornecedor_nome, local_livre, observacoes,
                valor_total, num_fatura, data_documento, empresa_nome,
                estado, criado_por, criado_em, atualizado_em, row_version
            FROM aprov.approval_hub
            WHERE id_approval_hub = @Id";

        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Id", id);

        using var reader = await cmd.ExecuteReaderAsync(ct);
        if (await reader.ReadAsync(ct))
        {
            return MapApprovalHub(reader);
        }

        return null;
    }

    public async Task<List<ApprovalHub>> GetAllApprovalHubsAsync(CancellationToken ct = default)
    {
        using var conn = await GetConnectionAsync(ct);

        const string sql = @"
            SELECT
                id_approval_hub, tipo_fatura, documento_a_criar, comprador, aprovador,
                fornecedor_bc_id, fornecedor_nome, local_livre, observacoes,
                valor_total, num_fatura, data_documento, empresa_nome,
                estado, criado_por, criado_em, atualizado_em, row_version
            FROM aprov.approval_hub
            ORDER BY criado_em DESC";

        var list = new List<ApprovalHub>();

        using var cmd = new SqlCommand(sql, conn);
        using var reader = await cmd.ExecuteReaderAsync(ct);

        while (await reader.ReadAsync(ct))
        {
            list.Add(MapApprovalHub(reader));
        }

        return list;
    }

    public async Task<List<ApprovalHub>> SearchApprovalHubsAsync(
        string? fornecedor = null,
        string? estado = null,
        DateTime? dataInicio = null,
        DateTime? dataFim = null,
        CancellationToken ct = default)
    {
        using var conn = await GetConnectionAsync(ct);

        var sql = @"
            SELECT
                id_approval_hub, tipo_fatura, documento_a_criar, comprador, aprovador,
                fornecedor_bc_id, fornecedor_nome, local_livre, observacoes,
                valor_total, num_fatura, data_documento, empresa_nome,
                estado, criado_por, criado_em, atualizado_em, row_version
            FROM aprov.approval_hub
            WHERE 1=1";

        if (!string.IsNullOrWhiteSpace(fornecedor))
            sql += " AND fornecedor_nome LIKE @Fornecedor";

        if (!string.IsNullOrWhiteSpace(estado))
            sql += " AND estado = @Estado";

        if (dataInicio.HasValue)
            sql += " AND data_documento >= @DataInicio";

        if (dataFim.HasValue)
            sql += " AND data_documento <= @DataFim";

        sql += " ORDER BY criado_em DESC";

        using var cmd = new SqlCommand(sql, conn);

        if (!string.IsNullOrWhiteSpace(fornecedor))
            cmd.Parameters.AddWithValue("@Fornecedor", $"%{fornecedor}%");

        if (!string.IsNullOrWhiteSpace(estado))
            cmd.Parameters.AddWithValue("@Estado", estado);

        if (dataInicio.HasValue)
            cmd.Parameters.AddWithValue("@DataInicio", dataInicio.Value);

        if (dataFim.HasValue)
            cmd.Parameters.AddWithValue("@DataFim", dataFim.Value);

        var list = new List<ApprovalHub>();

        using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            list.Add(MapApprovalHub(reader));
        }

        return list;
    }

    public async Task<bool> UpdateApprovalHubAsync(ApprovalHub approval, CancellationToken ct = default)
    {
        using var conn = await GetConnectionAsync(ct);

        const string sql = @"
            UPDATE aprov.approval_hub
            SET
                tipo_fatura = @TipoFatura,
                documento_a_criar = @DocumentoACriar,
                comprador = @Comprador,
                aprovador = @Aprovador,
                fornecedor_bc_id = @FornecedorBcId,
                fornecedor_nome = @FornecedorNome,
                local_livre = @LocalLivre,
                observacoes = @Observacoes,
                valor_total = @ValorTotal,
                num_fatura = @NumFatura,
                data_documento = @DataDocumento,
                empresa_nome = @EmpresaNome,
                estado = @Estado,
                atualizado_em = GETDATE()
            WHERE id_approval_hub = @Id";

        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Id", approval.IdApprovalHub);
        cmd.Parameters.AddWithValue("@TipoFatura", approval.TipoFatura ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@DocumentoACriar", approval.DocumentoACriar ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@Comprador", approval.Comprador ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@Aprovador", approval.Aprovador ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@FornecedorBcId", approval.FornecedorBcId ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@FornecedorNome", approval.FornecedorNome ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@LocalLivre", approval.LocalLivre ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@Observacoes", approval.Observacoes ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@ValorTotal", approval.ValorTotal);
        cmd.Parameters.AddWithValue("@NumFatura", approval.NumFatura ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@DataDocumento", approval.DataDocumento ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@EmpresaNome", approval.EmpresaNome ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@Estado", approval.Estado ?? (object)DBNull.Value);

        var rows = await cmd.ExecuteNonQueryAsync(ct);
        return rows > 0;
    }

    public async Task<bool> DeleteApprovalHubAsync(int id, CancellationToken ct = default)
    {
        using var conn = await GetConnectionAsync(ct);

        // Delete dimensões and anexos first (cascade)
        await DeleteDimensoesByApprovalHubIdAsync(id, ct);

        const string sqlAnexos = "DELETE FROM aprov.anexo WHERE id_approval_hub = @Id";
        using var cmdAnexos = new SqlCommand(sqlAnexos, conn);
        cmdAnexos.Parameters.AddWithValue("@Id", id);
        await cmdAnexos.ExecuteNonQueryAsync(ct);

        // Delete approval hub
        const string sql = "DELETE FROM aprov.approval_hub WHERE id_approval_hub = @Id";
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Id", id);

        var rows = await cmd.ExecuteNonQueryAsync(ct);
        return rows > 0;
    }

    public async Task<bool> UpdateEstadoAsync(int id, string estado, CancellationToken ct = default)
    {
        using var conn = await GetConnectionAsync(ct);

        const string sql = @"
            UPDATE aprov.approval_hub
            SET estado = @Estado, atualizado_em = GETDATE()
            WHERE id_approval_hub = @Id";

        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Id", id);
        cmd.Parameters.AddWithValue("@Estado", estado);

        var rows = await cmd.ExecuteNonQueryAsync(ct);
        return rows > 0;
    }

    #endregion

    #region Dimensões Operations

    public async Task<int> CreateDimensaoAsync(IdsDimensao dimensao, CancellationToken ct = default)
    {
        using var conn = await GetConnectionAsync(ct);

        const string sql = @"
            INSERT INTO aprov.ids_dimensoes (
                id_approval_hub, linha,
                produto_cod, produto_nome, produto_percentagem, produto_valor,
                centro_custo_cod, centro_custo_nome, centro_custo_percentagem, centro_custo_valor,
                cliente_cod, cliente_nome, cliente_percentagem, cliente_valor,
                marca_cod, marca_nome, marca_percentagem, marca_valor,
                mercado_cod, mercado_nome, mercado_percentagem, mercado_valor,
                projeto_cod, projeto_nome, projeto_percentagem, projeto_valor
            )
            OUTPUT INSERTED.id
            VALUES (
                @IdApprovalHub, @Linha,
                @ProdutoCod, @ProdutoNome, @ProdutoPercentagem, @ProdutoValor,
                @CentroCustoCod, @CentroCustoNome, @CentroCustoPercentagem, @CentroCustoValor,
                @ClienteCod, @ClienteNome, @ClientePercentagem, @ClienteValor,
                @MarcaCod, @MarcaNome, @MarcaPercentagem, @MarcaValor,
                @MercadoCod, @MercadoNome, @MercadoPercentagem, @MercadoValor,
                @ProjetoCod, @ProjetoNome, @ProjetoPercentagem, @ProjetoValor
            )";

        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@IdApprovalHub", dimensao.IdApprovalHub);
        cmd.Parameters.AddWithValue("@Linha", dimensao.Linha);

        // Produto
        cmd.Parameters.AddWithValue("@ProdutoCod", dimensao.ProdutoCod ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@ProdutoNome", dimensao.ProdutoNome ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@ProdutoPercentagem", dimensao.ProdutoPercentagem ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@ProdutoValor", dimensao.ProdutoValor ?? (object)DBNull.Value);

        // Centro de Custo
        cmd.Parameters.AddWithValue("@CentroCustoCod", dimensao.CentroCustoCod ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@CentroCustoNome", dimensao.CentroCustoNome ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@CentroCustoPercentagem", dimensao.CentroCustoPercentagem ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@CentroCustoValor", dimensao.CentroCustoValor ?? (object)DBNull.Value);

        // Cliente
        cmd.Parameters.AddWithValue("@ClienteCod", dimensao.ClienteCod ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@ClienteNome", dimensao.ClienteNome ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@ClientePercentagem", dimensao.ClientePercentagem ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@ClienteValor", dimensao.ClienteValor ?? (object)DBNull.Value);

        // Marca
        cmd.Parameters.AddWithValue("@MarcaCod", dimensao.MarcaCod ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@MarcaNome", dimensao.MarcaNome ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@MarcaPercentagem", dimensao.MarcaPercentagem ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@MarcaValor", dimensao.MarcaValor ?? (object)DBNull.Value);

        // Mercado
        cmd.Parameters.AddWithValue("@MercadoCod", dimensao.MercadoCod ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@MercadoNome", dimensao.MercadoNome ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@MercadoPercentagem", dimensao.MercadoPercentagem ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@MercadoValor", dimensao.MercadoValor ?? (object)DBNull.Value);

        // Projeto
        cmd.Parameters.AddWithValue("@ProjetoCod", dimensao.ProjetoCod ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@ProjetoNome", dimensao.ProjetoNome ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@ProjetoPercentagem", dimensao.ProjetoPercentagem ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@ProjetoValor", dimensao.ProjetoValor ?? (object)DBNull.Value);

        var result = await cmd.ExecuteScalarAsync(ct);
        return result != null ? Convert.ToInt32(result) : 0;
    }

    public async Task<List<IdsDimensao>> GetDimensoesByApprovalHubIdAsync(int approvalHubId, CancellationToken ct = default)
    {
        using var conn = await GetConnectionAsync(ct);

        const string sql = @"
            SELECT
                id, id_approval_hub, linha,
                produto_cod, produto_nome, produto_percentagem, produto_valor,
                centro_custo_cod, centro_custo_nome, centro_custo_percentagem, centro_custo_valor,
                cliente_cod, cliente_nome, cliente_percentagem, cliente_valor,
                marca_cod, marca_nome, marca_percentagem, marca_valor,
                mercado_cod, mercado_nome, mercado_percentagem, mercado_valor,
                projeto_cod, projeto_nome, projeto_percentagem, projeto_valor
            FROM aprov.ids_dimensoes
            WHERE id_approval_hub = @ApprovalHubId
            ORDER BY linha";

        var list = new List<IdsDimensao>();

        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@ApprovalHubId", approvalHubId);

        using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            list.Add(MapDimensao(reader));
        }

        return list;
    }

    public async Task<bool> DeleteDimensaoAsync(int id, CancellationToken ct = default)
    {
        using var conn = await GetConnectionAsync(ct);

        const string sql = "DELETE FROM aprov.ids_dimensoes WHERE id = @Id";
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Id", id);

        var rows = await cmd.ExecuteNonQueryAsync(ct);
        return rows > 0;
    }

    public async Task<bool> DeleteDimensoesByApprovalHubIdAsync(int approvalHubId, CancellationToken ct = default)
    {
        using var conn = await GetConnectionAsync(ct);

        const string sql = "DELETE FROM aprov.ids_dimensoes WHERE id_approval_hub = @ApprovalHubId";
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@ApprovalHubId", approvalHubId);

        await cmd.ExecuteNonQueryAsync(ct);
        return true;
    }

    #endregion

    #region Anexos Operations

    public async Task<int> CreateAnexoAsync(Anexo anexo, CancellationToken ct = default)
    {
        using var conn = await GetConnectionAsync(ct);

        const string sql = @"
            INSERT INTO aprov.anexo (
                id_approval_hub, nome_ficheiro, content_type, tamanho_bytes, conteudo, carregado_por, carregado_em
            )
            OUTPUT INSERTED.id_anexo
            VALUES (
                @IdApprovalHub, @NomeFicheiro, @ContentType, @TamanhoBytes, @Conteudo, @CarregadoPor, GETDATE()
            )";

        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@IdApprovalHub", anexo.IdApprovalHub);
        cmd.Parameters.AddWithValue("@NomeFicheiro", anexo.NomeFicheiro ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@ContentType", anexo.ContentType ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@TamanhoBytes", anexo.TamanhoBytes);
        cmd.Parameters.AddWithValue("@Conteudo", anexo.Conteudo ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@CarregadoPor", anexo.CarregadoPor ?? GetCurrentUser());

        var result = await cmd.ExecuteScalarAsync(ct);
        return result != null ? Convert.ToInt32(result) : 0;
    }

    public async Task<Anexo?> GetAnexoByIdAsync(int id, CancellationToken ct = default)
    {
        using var conn = await GetConnectionAsync(ct);

        const string sql = @"
            SELECT id_anexo, id_approval_hub, nome_ficheiro, content_type, tamanho_bytes, conteudo, carregado_por, carregado_em
            FROM aprov.anexo
            WHERE id_anexo = @Id";

        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Id", id);

        using var reader = await cmd.ExecuteReaderAsync(ct);
        if (await reader.ReadAsync(ct))
        {
            return MapAnexo(reader);
        }

        return null;
    }

    public async Task<List<Anexo>> GetAnexosByApprovalHubIdAsync(int approvalHubId, CancellationToken ct = default)
    {
        using var conn = await GetConnectionAsync(ct);

        const string sql = @"
            SELECT id_anexo, id_approval_hub, nome_ficheiro, content_type, tamanho_bytes, conteudo, carregado_por, carregado_em
            FROM aprov.anexo
            WHERE id_approval_hub = @ApprovalHubId
            ORDER BY carregado_em DESC";

        var list = new List<Anexo>();

        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@ApprovalHubId", approvalHubId);

        using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            list.Add(MapAnexo(reader, includeContent: false)); // Don't load full content for list
        }

        return list;
    }

    public async Task<bool> DeleteAnexoAsync(int id, CancellationToken ct = default)
    {
        using var conn = await GetConnectionAsync(ct);

        const string sql = "DELETE FROM aprov.anexo WHERE id_anexo = @Id";
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Id", id);

        var rows = await cmd.ExecuteNonQueryAsync(ct);
        return rows > 0;
    }

    #endregion

    #region Utility Methods

    public async Task<List<string>> GetDistinctFornecedoresAsync(CancellationToken ct = default)
    {
        using var conn = await GetConnectionAsync(ct);

        const string sql = @"
            SELECT DISTINCT fornecedor_nome
            FROM aprov.approval_hub
            WHERE fornecedor_nome IS NOT NULL
            ORDER BY fornecedor_nome";

        var list = new List<string>();

        using var cmd = new SqlCommand(sql, conn);
        using var reader = await cmd.ExecuteReaderAsync(ct);

        while (await reader.ReadAsync(ct))
        {
            list.Add(reader.GetString(0));
        }

        return list;
    }

    public async Task<List<string>> GetDistinctEstadosAsync(CancellationToken ct = default)
    {
        using var conn = await GetConnectionAsync(ct);

        const string sql = @"
            SELECT DISTINCT estado
            FROM aprov.approval_hub
            WHERE estado IS NOT NULL
            ORDER BY estado";

        var list = new List<string>();

        using var cmd = new SqlCommand(sql, conn);
        using var reader = await cmd.ExecuteReaderAsync(ct);

        while (await reader.ReadAsync(ct))
        {
            list.Add(reader.GetString(0));
        }

        return list;
    }

    #endregion

    #region Mapping Methods

    private ApprovalHub MapApprovalHub(SqlDataReader reader)
    {
        return new ApprovalHub
        {
            IdApprovalHub = reader.GetInt32(reader.GetOrdinal("id_approval_hub")),
            TipoFatura = reader.IsDBNull(reader.GetOrdinal("tipo_fatura")) ? null : reader.GetString(reader.GetOrdinal("tipo_fatura")),
            DocumentoACriar = reader.IsDBNull(reader.GetOrdinal("documento_a_criar")) ? null : reader.GetString(reader.GetOrdinal("documento_a_criar")),
            Comprador = reader.IsDBNull(reader.GetOrdinal("comprador")) ? null : reader.GetString(reader.GetOrdinal("comprador")),
            Aprovador = reader.IsDBNull(reader.GetOrdinal("aprovador")) ? null : reader.GetString(reader.GetOrdinal("aprovador")),
            FornecedorBcId = reader.IsDBNull(reader.GetOrdinal("fornecedor_bc_id")) ? null : reader.GetString(reader.GetOrdinal("fornecedor_bc_id")),
            FornecedorNome = reader.IsDBNull(reader.GetOrdinal("fornecedor_nome")) ? null : reader.GetString(reader.GetOrdinal("fornecedor_nome")),
            LocalLivre = reader.IsDBNull(reader.GetOrdinal("local_livre")) ? null : reader.GetString(reader.GetOrdinal("local_livre")),
            Observacoes = reader.IsDBNull(reader.GetOrdinal("observacoes")) ? null : reader.GetString(reader.GetOrdinal("observacoes")),
            ValorTotal = reader.GetDecimal(reader.GetOrdinal("valor_total")),
            NumFatura = reader.IsDBNull(reader.GetOrdinal("num_fatura")) ? null : reader.GetString(reader.GetOrdinal("num_fatura")),
            DataDocumento = reader.IsDBNull(reader.GetOrdinal("data_documento")) ? null : reader.GetDateTime(reader.GetOrdinal("data_documento")),
            EmpresaNome = reader.IsDBNull(reader.GetOrdinal("empresa_nome")) ? null : reader.GetString(reader.GetOrdinal("empresa_nome")),
            Estado = reader.IsDBNull(reader.GetOrdinal("estado")) ? null : reader.GetString(reader.GetOrdinal("estado")),
            CriadoPor = reader.IsDBNull(reader.GetOrdinal("criado_por")) ? null : reader.GetString(reader.GetOrdinal("criado_por")),
            CriadoEm = reader.IsDBNull(reader.GetOrdinal("criado_em")) ? null : reader.GetDateTime(reader.GetOrdinal("criado_em")),
            AtualizadoEm = reader.IsDBNull(reader.GetOrdinal("atualizado_em")) ? null : reader.GetDateTime(reader.GetOrdinal("atualizado_em")),
            RowVersion = reader.IsDBNull(reader.GetOrdinal("row_version")) ? null : (byte[])reader["row_version"]
        };
    }

    private IdsDimensao MapDimensao(SqlDataReader reader)
    {
        return new IdsDimensao
        {
            Id = reader.GetInt32(reader.GetOrdinal("id")),
            IdApprovalHub = reader.GetInt32(reader.GetOrdinal("id_approval_hub")),
            Linha = reader.GetInt32(reader.GetOrdinal("linha")),

            // Produto
            ProdutoCod = reader.IsDBNull(reader.GetOrdinal("produto_cod")) ? null : reader.GetString(reader.GetOrdinal("produto_cod")),
            ProdutoNome = reader.IsDBNull(reader.GetOrdinal("produto_nome")) ? null : reader.GetString(reader.GetOrdinal("produto_nome")),
            ProdutoPercentagem = reader.IsDBNull(reader.GetOrdinal("produto_percentagem")) ? null : reader.GetDecimal(reader.GetOrdinal("produto_percentagem")),
            ProdutoValor = reader.IsDBNull(reader.GetOrdinal("produto_valor")) ? null : reader.GetDecimal(reader.GetOrdinal("produto_valor")),

            // Centro de Custo
            CentroCustoCod = reader.IsDBNull(reader.GetOrdinal("centro_custo_cod")) ? null : reader.GetString(reader.GetOrdinal("centro_custo_cod")),
            CentroCustoNome = reader.IsDBNull(reader.GetOrdinal("centro_custo_nome")) ? null : reader.GetString(reader.GetOrdinal("centro_custo_nome")),
            CentroCustoPercentagem = reader.IsDBNull(reader.GetOrdinal("centro_custo_percentagem")) ? null : reader.GetDecimal(reader.GetOrdinal("centro_custo_percentagem")),
            CentroCustoValor = reader.IsDBNull(reader.GetOrdinal("centro_custo_valor")) ? null : reader.GetDecimal(reader.GetOrdinal("centro_custo_valor")),

            // Cliente
            ClienteCod = reader.IsDBNull(reader.GetOrdinal("cliente_cod")) ? null : reader.GetString(reader.GetOrdinal("cliente_cod")),
            ClienteNome = reader.IsDBNull(reader.GetOrdinal("cliente_nome")) ? null : reader.GetString(reader.GetOrdinal("cliente_nome")),
            ClientePercentagem = reader.IsDBNull(reader.GetOrdinal("cliente_percentagem")) ? null : reader.GetDecimal(reader.GetOrdinal("cliente_percentagem")),
            ClienteValor = reader.IsDBNull(reader.GetOrdinal("cliente_valor")) ? null : reader.GetDecimal(reader.GetOrdinal("cliente_valor")),

            // Marca
            MarcaCod = reader.IsDBNull(reader.GetOrdinal("marca_cod")) ? null : reader.GetString(reader.GetOrdinal("marca_cod")),
            MarcaNome = reader.IsDBNull(reader.GetOrdinal("marca_nome")) ? null : reader.GetString(reader.GetOrdinal("marca_nome")),
            MarcaPercentagem = reader.IsDBNull(reader.GetOrdinal("marca_percentagem")) ? null : reader.GetDecimal(reader.GetOrdinal("marca_percentagem")),
            MarcaValor = reader.IsDBNull(reader.GetOrdinal("marca_valor")) ? null : reader.GetDecimal(reader.GetOrdinal("marca_valor")),

            // Mercado
            MercadoCod = reader.IsDBNull(reader.GetOrdinal("mercado_cod")) ? null : reader.GetString(reader.GetOrdinal("mercado_cod")),
            MercadoNome = reader.IsDBNull(reader.GetOrdinal("mercado_nome")) ? null : reader.GetString(reader.GetOrdinal("mercado_nome")),
            MercadoPercentagem = reader.IsDBNull(reader.GetOrdinal("mercado_percentagem")) ? null : reader.GetDecimal(reader.GetOrdinal("mercado_percentagem")),
            MercadoValor = reader.IsDBNull(reader.GetOrdinal("mercado_valor")) ? null : reader.GetDecimal(reader.GetOrdinal("mercado_valor")),

            // Projeto
            ProjetoCod = reader.IsDBNull(reader.GetOrdinal("projeto_cod")) ? null : reader.GetString(reader.GetOrdinal("projeto_cod")),
            ProjetoNome = reader.IsDBNull(reader.GetOrdinal("projeto_nome")) ? null : reader.GetString(reader.GetOrdinal("projeto_nome")),
            ProjetoPercentagem = reader.IsDBNull(reader.GetOrdinal("projeto_percentagem")) ? null : reader.GetDecimal(reader.GetOrdinal("projeto_percentagem")),
            ProjetoValor = reader.IsDBNull(reader.GetOrdinal("projeto_valor")) ? null : reader.GetDecimal(reader.GetOrdinal("projeto_valor"))
        };
    }

    private Anexo MapAnexo(SqlDataReader reader, bool includeContent = true)
    {
        var anexo = new Anexo
        {
            IdAnexo = reader.GetInt32(reader.GetOrdinal("id_anexo")),
            IdApprovalHub = reader.GetInt32(reader.GetOrdinal("id_approval_hub")),
            NomeFicheiro = reader.IsDBNull(reader.GetOrdinal("nome_ficheiro")) ? null : reader.GetString(reader.GetOrdinal("nome_ficheiro")),
            ContentType = reader.IsDBNull(reader.GetOrdinal("content_type")) ? null : reader.GetString(reader.GetOrdinal("content_type")),
            TamanhoBytes = reader.GetInt64(reader.GetOrdinal("tamanho_bytes")),
            CarregadoPor = reader.IsDBNull(reader.GetOrdinal("carregado_por")) ? null : reader.GetString(reader.GetOrdinal("carregado_por")),
            CarregadoEm = reader.IsDBNull(reader.GetOrdinal("carregado_em")) ? null : reader.GetDateTime(reader.GetOrdinal("carregado_em"))
        };

        if (includeContent && !reader.IsDBNull(reader.GetOrdinal("conteudo")))
        {
            anexo.Conteudo = (byte[])reader["conteudo"];
        }

        return anexo;
    }

    #endregion
}
