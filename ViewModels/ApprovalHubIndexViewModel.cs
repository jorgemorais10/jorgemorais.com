using hub.Models;

namespace Hub.ViewModels;

public class ApprovalHubGroupedViewModel
{
    public string? Aprovador { get; set; }
    public List<ApprovalHub> Pedidos { get; set; } = new();
    public int TotalPedidos => Pedidos.Count;
    public decimal ValorTotal => Pedidos.Sum(p => p.ValorTotal);
    public int PedidosPendentes => Pedidos.Count(p => p.Estado == "Pendente");
    public int PedidosAprovados => Pedidos.Count(p => p.Estado == "Aprovado");
    public int PedidosRejeitados => Pedidos.Count(p => p.Estado == "Rejeitado");
    public int PedidosEmAnalise => Pedidos.Count(p => p.Estado == "Em Análise");
}

public class ApprovalHubIndexViewModel
{
    public List<ApprovalHubGroupedViewModel> GruposPorAprovador { get; set; } = new();
    public int TotalPedidos => GruposPorAprovador.Sum(g => g.TotalPedidos);
    public decimal ValorTotalGeral => GruposPorAprovador.Sum(g => g.ValorTotal);

    // Para a pesquisa
    public string? Fornecedor { get; set; }
    public string? Estado { get; set; }
    public DateTime? DataInicio { get; set; }
    public DateTime? DataFim { get; set; }
}
