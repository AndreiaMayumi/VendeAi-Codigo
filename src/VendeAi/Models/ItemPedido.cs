
namespace VendeAi.Models;

public class ItemPedido
{
    public int Id { get; set; }

    public required Produto Produto { get; set; }

    // Variação selecionada, quando existir.
    public ProdutoVariacao? ProdutoVariacao { get; set; }

    public required ProdutoModalidade ProdutoModalidade { get; set; }

    public int Quantidade { get; set; }

    // Preço registrado no momento da venda.
    public decimal PrecoUnitario { get; set; }

    public decimal Subtotal => Quantidade * PrecoUnitario;
}
