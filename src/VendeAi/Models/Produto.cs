
namespace VendeAi.Models;

public enum Modalidade
{
    Varejo,
    Atacado,
    Pacote,
    Lote
}

public enum StatusProduto
{
    Ativo,
    Inativo
}

public enum Categoria
{
    Roupas,
    Cosmeticos,
    Acessorios
}

public class Produto
{
    public required string Nome { get; set; }

    // Identificador único do produto
    public required string CodigoSku { get; set; }

    public string? Descricao { get; set; }

    public required Categoria Categoria { get; set; }

    public string? Marca { get; set; }

    public string? Imagem { get; set; }

    public StatusProduto Status { get; set; }

    public List<ProdutoModalidade> Modalidades { get; set; } = new();
}
