using System.ComponentModel.DataAnnotations;

namespace hub.Models;

/// <summary>
/// Representa uma linha completa de dimensões onde Produto é a chave principal
/// Cada linha pode conter: Produto, Centro de Custo, Cliente, Marca, Mercado e Projeto
/// </summary>
public class IdsDimensao
{
    public int Id { get; set; }

    [Required]
    public int IdApprovalHub { get; set; }

    [Display(Name = "Linha")]
    public int Linha { get; set; }

    // Produto (chave principal - obrigatório)
    [Display(Name = "Código Produto")]
    public string? ProdutoCod { get; set; }

    [Display(Name = "Nome Produto")]
    public string? ProdutoNome { get; set; }

    [Display(Name = "% Produto")]
    [Range(0, 100, ErrorMessage = "Percentagem deve estar entre 0 e 100")]
    public decimal? ProdutoPercentagem { get; set; }

    [Display(Name = "Valor Produto")]
    [Range(0, double.MaxValue, ErrorMessage = "Valor não pode ser negativo")]
    public decimal? ProdutoValor { get; set; }

    // Centro de Custo
    [Display(Name = "Código Centro Custo")]
    public string? CentroCustoCod { get; set; }

    [Display(Name = "Nome Centro Custo")]
    public string? CentroCustoNome { get; set; }

    [Display(Name = "% Centro Custo")]
    [Range(0, 100, ErrorMessage = "Percentagem deve estar entre 0 e 100")]
    public decimal? CentroCustoPercentagem { get; set; }

    [Display(Name = "Valor Centro Custo")]
    [Range(0, double.MaxValue, ErrorMessage = "Valor não pode ser negativo")]
    public decimal? CentroCustoValor { get; set; }

    // Cliente
    [Display(Name = "Código Cliente")]
    public string? ClienteCod { get; set; }

    [Display(Name = "Nome Cliente")]
    public string? ClienteNome { get; set; }

    [Display(Name = "% Cliente")]
    [Range(0, 100, ErrorMessage = "Percentagem deve estar entre 0 e 100")]
    public decimal? ClientePercentagem { get; set; }

    [Display(Name = "Valor Cliente")]
    [Range(0, double.MaxValue, ErrorMessage = "Valor não pode ser negativo")]
    public decimal? ClienteValor { get; set; }

    // Marca
    [Display(Name = "Código Marca")]
    public string? MarcaCod { get; set; }

    [Display(Name = "Nome Marca")]
    public string? MarcaNome { get; set; }

    [Display(Name = "% Marca")]
    [Range(0, 100, ErrorMessage = "Percentagem deve estar entre 0 e 100")]
    public decimal? MarcaPercentagem { get; set; }

    [Display(Name = "Valor Marca")]
    [Range(0, double.MaxValue, ErrorMessage = "Valor não pode ser negativo")]
    public decimal? MarcaValor { get; set; }

    // Mercado
    [Display(Name = "Código Mercado")]
    public string? MercadoCod { get; set; }

    [Display(Name = "Nome Mercado")]
    public string? MercadoNome { get; set; }

    [Display(Name = "% Mercado")]
    [Range(0, 100, ErrorMessage = "Percentagem deve estar entre 0 e 100")]
    public decimal? MercadoPercentagem { get; set; }

    [Display(Name = "Valor Mercado")]
    [Range(0, double.MaxValue, ErrorMessage = "Valor não pode ser negativo")]
    public decimal? MercadoValor { get; set; }

    // Projeto
    [Display(Name = "Código Projeto")]
    public string? ProjetoCod { get; set; }

    [Display(Name = "Nome Projeto")]
    public string? ProjetoNome { get; set; }

    [Display(Name = "% Projeto")]
    [Range(0, 100, ErrorMessage = "Percentagem deve estar entre 0 e 100")]
    public decimal? ProjetoPercentagem { get; set; }

    [Display(Name = "Valor Projeto")]
    [Range(0, double.MaxValue, ErrorMessage = "Valor não pode ser negativo")]
    public decimal? ProjetoValor { get; set; }

    public DateTime? ValidoDe { get; set; }
    public DateTime? ValidoAte { get; set; }

    // Navigation property
    public ApprovalHub? ApprovalHub { get; set; }

    // Helper property to display in UI
    public string DisplayName => !string.IsNullOrWhiteSpace(ProdutoNome)
        ? $"Linha {Linha}: {ProdutoNome}"
        : $"Linha {Linha}";
}
