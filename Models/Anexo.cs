using System.ComponentModel.DataAnnotations;

namespace hub.Models;

public class Anexo
{
    public int IdAnexo { get; set; }

    [Required]
    public int IdApprovalHub { get; set; }

    [Required(ErrorMessage = "Nome do ficheiro é obrigatório")]
    [Display(Name = "Nome do Ficheiro")]
    public string? NomeFicheiro { get; set; }

    [Display(Name = "Tipo de Conteúdo")]
    public string? ContentType { get; set; }

    [Display(Name = "Tamanho (bytes)")]
    public long TamanhoBytes { get; set; }

    [Display(Name = "Conteúdo")]
    public byte[]? Conteudo { get; set; }

    [Display(Name = "Carregado Por")]
    public string? CarregadoPor { get; set; }

    [Display(Name = "Carregado Em")]
    public DateTime? CarregadoEm { get; set; }

    // Navigation property
    public ApprovalHub? ApprovalHub { get; set; }
}
