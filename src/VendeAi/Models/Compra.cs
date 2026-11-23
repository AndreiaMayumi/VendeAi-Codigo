namespace VendeAi.Models;

// Linha consolidada para acompanhamento de compras.
public class Compra
{
    public string Chave { get; set; } = "";
    public string Produto { get; set; } = "";
    public string Variacao { get; set; } = "";
    public string Categoria { get; set; } = "";
    public int Quantidade { get; set; }
    public StatusCompra Status { get; set; } = StatusCompra.Aberto;
}
