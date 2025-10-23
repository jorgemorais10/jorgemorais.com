using System.ComponentModel.DataAnnotations;

namespace hub.Models;

public class IdsDimensao
{
    public int Id { get; set; }

    [Required]
    public int IdApprovalHub { get; set; }

    [Display(Name = "Linha")]
    public int Linha { get; set; }

    [Required(ErrorMessage = "Tipo de dimensão é obrigatório")]
    [Display(Name = "Tipo")]
    public string? Tipo { get; set; }

    [Display(Name = "Código da Dimensão")]
    public string? CodDimensao { get; set; }

    [Display(Name = "Nome da Dimensão")]
    public string? NomeDimensao { get; set; }

    [Display(Name = "Percentagem")]
    [Range(0, 100, ErrorMessage = "Percentagem deve estar entre 0 e 100")]
    public decimal? Percentagem { get; set; }

    [Display(Name = "Valor")]
    [Range(0, double.MaxValue, ErrorMessage = "Valor não pode ser negativo")]
    public decimal? Valor { get; set; }

    public DateTime? ValidoDe { get; set; }
    public DateTime? ValidoAte { get; set; }

    // Navigation property
    public ApprovalHub? ApprovalHub { get; set; }
}

public enum TipoDimensao
{
    CentroCusto,
    Cliente,
    Produto,
    Marca,
    Mercado,
    Projeto
}
