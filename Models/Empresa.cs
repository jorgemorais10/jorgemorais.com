using Microsoft.Data.SqlClient;

namespace hub.Models;

public class Empresa
{
    public int Empresa_ID { get; set; }
    public string? Nome { get; set; }
    public int? EmpresaDefeito_ID { get; set; }
    public int? Aplicacao_ID { get; set; }

    public static List<Empresa> GetEmpresas(string username, string connectionString)
    {
        var empresas = new List<Empresa>();

        using var conn = new SqlConnection(connectionString);
        conn.Open();

        var sql = @"
            SELECT e.Empresa_ID, e.Nome, u.EmpresaDefeito_ID, a.Aplicacao_ID
            FROM CONFIG_EMPRESA e
            INNER JOIN CONFIG_UTILIZADOR u ON u.Utilizador = @username
            LEFT JOIN CONFIG_APLICACAO a ON a.Empresa_ID = e.Empresa_ID
            WHERE e.Activo = 1";

        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@username", username);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            empresas.Add(new Empresa
            {
                Empresa_ID = reader.GetInt32(0),
                Nome = reader.IsDBNull(1) ? null : reader.GetString(1),
                EmpresaDefeito_ID = reader.IsDBNull(2) ? null : reader.GetInt32(2),
                Aplicacao_ID = reader.IsDBNull(3) ? null : reader.GetInt32(3)
            });
        }

        return empresas;
    }
}
