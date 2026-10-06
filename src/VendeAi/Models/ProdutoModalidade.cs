namespace VendeAi.Models;

public class ProdutoModalidade
{
    public ModalidadeVenda Modalidade { get; set; }

    public int QuantidadeMinimaVenda { get; set; }

    public decimal PrecoVenda { get; set; }

    public decimal? PrecoCusto { get; set; }
}