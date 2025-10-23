using System.ComponentModel.DataAnnotations;

namespace hub.Models;

public class ApprovalHub
{
    public int IdApprovalHub { get; set; }

    [Required(ErrorMessage = "Tipo de fatura é obrigatório")]
    [Display(Name = "Tipo de Fatura")]
    public string? TipoFatura { get; set; }

    [Display(Name = "Documento a Criar")]
    public string? DocumentoACriar { get; set; }

    [Display(Name = "Comprador")]
    public string? Comprador { get; set; }

    [Display(Name = "Aprovador")]
    public string? Aprovador { get; set; }

    [Display(Name = "ID Fornecedor BC")]
    public string? FornecedorBcId { get; set; }

    [Required(ErrorMessage = "Nome do fornecedor é obrigatório")]
    [Display(Name = "Nome do Fornecedor")]
    public string? FornecedorNome { get; set; }

    [Display(Name = "Local Livre")]
    public string? LocalLivre { get; set; }

    [Display(Name = "Observações")]
    public string? Observacoes { get; set; }

    [Required(ErrorMessage = "Valor total é obrigatório")]
    [Display(Name = "Valor Total")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Valor deve ser maior que zero")]
    public decimal ValorTotal { get; set; }

    [Display(Name = "Nº Fatura")]
    public string? NumFatura { get; set; }

    [Display(Name = "Data do Documento")]
    [DataType(DataType.Date)]
    public DateTime? DataDocumento { get; set; }

    [Display(Name = "Empresa")]
    public string? EmpresaNome { get; set; }

    [Display(Name = "Estado")]
    public string? Estado { get; set; }

    [Display(Name = "Criado Por")]
    public string? CriadoPor { get; set; }

    [Display(Name = "Criado Em")]
    public DateTime? CriadoEm { get; set; }

    [Display(Name = "Atualizado Em")]
    public DateTime? AtualizadoEm { get; set; }

    public byte[]? RowVersion { get; set; }
    public DateTime? ValidoDe { get; set; }
    public DateTime? ValidoAte { get; set; }

    // Navigation properties
    public List<IdsDimensao>? Dimensoes { get; set; }
    public List<Anexo>? Anexos { get; set; }
}
