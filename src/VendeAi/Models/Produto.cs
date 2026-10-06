namespace VendeAi.Models;

public enum ModalidadeVenda
{
    Varejo,
    Atacado,
    PacoteInternacional,
    LoteNacional
}

public enum StatusProduto
{
    Ativo,
    Inativo
}

public class Produto
{
    public required string Nome { get; set; }

    public required string CodigoSku { get; set; }

    public string? Descricao { get; set; }

    public required string Categoria { get; set; }

    public string? Marca { get; set; }

    public string? Imagem { get; set; }

    public StatusProduto Status { get; set; }

    public List<ProdutoModalidade> Modalidades { get; set; } = new();
}